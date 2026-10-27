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
    {
        Label: string
        Icon: string option
        Action: unit -> unit
    }

/// Notification request. Input to create a notification.
type NotificationRequest =
    {
        Type: NotificationType
        Channel: NotificationChannel
        Title: string
        Message: string
        Details: string option
        Icon: string option
        Actions: NotificationAction list
        AutoDismissAfter: TimeSpan option
    }

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
        {
            Type = ntype |> Option.defaultValue NotificationType.Info
            Title = title
            Message = message
            Details = details
            Icon = icon
            Actions = action |> Option.toList
            AutoDismissAfter = autoDismissAfter
            Channel = channel |> Option.defaultValue NotificationChannel.Toast
        }

/// Notification. A user alert or message.
type Notification =
    {
        NotificationId: Guid
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
        IsPinned: bool
    }

/// NotificationEngine record. Manages all notifications.
type NotificationService =
    {
        Show: NotificationRequest -> Guid
        Dismiss: Guid -> unit
        DismissAll: NotificationChannel -> unit
        MarkAsRead: Guid -> unit
        GetHistory: unit -> Notification list
        Subscribe: (Notification -> unit) -> IDisposable
    }

module NotificationService =

    type private NotificationMsg =
        | Show of NotificationRequest * AsyncReplyChannel<Guid>
        | Dismiss of Guid
        | DismissAll of NotificationChannel
        | MarkAsRead of Guid
        | GetHistory of AsyncReplyChannel<Notification list>
        | Subscribe of (Notification -> unit) * AsyncReplyChannel<IDisposable>
        | Unsubscribe of int

    let create () : NotificationService =
        let agent =
            MailboxProcessor.Start(fun inbox ->
                let rec loop
                    (nextSubId: int)
                    (notifications: Map<Guid, Notification>)
                    (listeners: Map<int, Notification -> unit>)
                    =
                    async {
                        let! msg = inbox.Receive()

                        match msg with
                        | Show(request, reply) ->
                            let id = Guid.NewGuid()

                            let notification: Notification =
                                {
                                    NotificationId = id
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
                                    IsPinned = false
                                }

                            for KeyValue(_, listener) in listeners do
                                try
                                    listener notification
                                with ex ->
                                    printfn "[NotificationService] Listener error: %s" ex.Message

                            reply.Reply id
                            return! loop nextSubId (Map.add id notification notifications) listeners

                        | Dismiss id -> return! loop nextSubId (Map.remove id notifications) listeners

                        | DismissAll channel ->
                            let filtered = notifications |> Map.filter(fun _ n -> n.Channel <> channel)
                            return! loop nextSubId filtered listeners

                        | MarkAsRead id ->
                            let updated =
                                notifications |> Map.change id (Option.map(fun n -> { n with IsRead = true }))

                            return! loop nextSubId updated listeners

                        | GetHistory reply ->
                            reply.Reply(notifications |> Map.values |> Seq.toList)
                            return! loop nextSubId notifications listeners

                        | Subscribe(listener, reply) ->
                            let subId = nextSubId

                            let disposable =
                                { new IDisposable with
                                    member _.Dispose() = inbox.Post(Unsubscribe subId)
                                }

                            reply.Reply disposable
                            return! loop (nextSubId + 1) notifications (Map.add subId listener listeners)

                        | Unsubscribe subId -> return! loop nextSubId notifications (Map.remove subId listeners)
                    }

                loop 0 Map.empty Map.empty)

        {
            Show = fun req -> agent.PostAndReply(fun reply -> Show(req, reply))
            Dismiss = fun id -> agent.Post(Dismiss id)
            DismissAll = fun ch -> agent.Post(DismissAll ch)
            MarkAsRead = fun id -> agent.Post(MarkAsRead id)
            GetHistory = fun () -> agent.PostAndReply GetHistory
            Subscribe = fun listener -> agent.PostAndReply(fun reply -> Subscribe(listener, reply))
        }
