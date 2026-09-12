namespace Medhavi.Client.Services

open System

/// Notification type. Severity classification.
type NotificationType =
    | Error' = 0
    | Info = 1
    | Success = 2
    | Warning = 3

/// Notification channel. Where the notification is displayed.
type NotificationChannel =
    | Toast // Auto-dismiss, top-right
    | Banner // Persistent, top-center
    | Alert // Modal, requires action
    | Center // Notification center (history)

/// Notification action. Button on a notification.
type NotificationAction =
    { Label: string
      Icon: string option
      Action: unit -> unit }

/// Notification request. Input to create a notification.
type NotificationRequest =
    { Type: NotificationType
      Channel: NotificationChannel
      Title: string
      Message: string
      Details: string option
      Icon: string option
      Actions: NotificationAction list
      AutoDismissAfter: TimeSpan option }

    static member Create
        (
            title: string,
            message: string,
            ?ntype: NotificationType,
            ?details: string,
            ?icon: string,
            ?action: NotificationAction,
            ?autoDismissAfter: TimeSpan,
            ?channel: NotificationChannel
        ) =
        { Type = ntype |> Option.defaultValue NotificationType.Info
          Title = title
          Message = message
          Details = details
          Icon = icon
          Actions = action |> Option.toList
          AutoDismissAfter = autoDismissAfter
          Channel = channel |> Option.defaultValue NotificationChannel.Toast }

/// Notification. A user alert or message.
type Notification =
    { NotificationId: Guid
      Type: NotificationType
      Channel: NotificationChannel
      Title: string
      Message: string
      Details: string option
      Icon: string option
      Actions: NotificationAction list
      CreatedAt: DateTimeOffset
      AutoDismissAfter: TimeSpan option
      IsRead: bool
      IsPinned: bool }

/// NotificationEngine record. Manages all notifications.
type NotificationService =
    { Show: NotificationRequest -> Guid
      Dismiss: Guid -> unit
      DismissAll: NotificationChannel -> unit
      MarkAsRead: Guid -> unit
      GetHistory: unit -> Notification list
      Subscribe: (Notification -> unit) -> IDisposable }

module NotificationService =
    let create () : NotificationService =
        let mutable nextId = 0
        let mutable notifications: Map<Guid, Notification> = Map.empty
        let mutable listeners: Map<int, Notification -> unit> = Map.empty
        let lockObj = obj()

        let notifySubscribers (notification: Notification) : unit =
            lock lockObj (fun () ->
                listeners
                |> Map.iter(fun _ listener ->
                    try
                        listener notification
                    with ex ->
                        printfn "[NotificationService] Listener error: %s" ex.Message))

        let show (request: NotificationRequest) : Guid =
            let id = Guid.NewGuid()

            let notification: Notification =
                { NotificationId = id
                  Type = request.Type
                  Channel = request.Channel
                  Title = request.Title
                  Message = request.Message
                  Details = request.Details
                  Icon = request.Icon
                  Actions = request.Actions
                  CreatedAt = DateTimeOffset.UtcNow
                  AutoDismissAfter = request.AutoDismissAfter
                  IsRead = false
                  IsPinned = false }

            lock lockObj (fun () -> notifications <- notifications |> Map.add id notification)
            notifySubscribers notification
            id

        let dismiss (id: Guid) : unit = lock lockObj (fun () -> notifications <- notifications |> Map.remove id)

        let dismissAll (channel: NotificationChannel) : unit =
            lock lockObj (fun () -> notifications <- notifications |> Map.filter(fun _ n -> n.Channel <> channel))

        let markAsRead (id: Guid) : unit =
            lock lockObj (fun () ->
                notifications <- notifications |> Map.change id (Option.map(fun n -> { n with IsRead = true })))

        let getHistory () : Notification list = lock lockObj (fun () -> notifications |> Map.values |> Seq.toList)

        let subscribe (listener: Notification -> unit) : IDisposable =
            let id =
                lock lockObj (fun () ->
                    let id = nextId
                    nextId <- nextId + 1
                    listeners <- listeners |> Map.add id listener
                    id)

            { new IDisposable with
                member _.Dispose() = lock lockObj (fun () -> listeners <- listeners |> Map.remove id) }

        { Show = show
          Dismiss = dismiss
          DismissAll = dismissAll
          MarkAsRead = markAsRead
          GetHistory = getHistory
          Subscribe = subscribe }
