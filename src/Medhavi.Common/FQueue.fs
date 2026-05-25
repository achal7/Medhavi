namespace Medhavi.Common

/// Two-list persistent queue with amortized O(1) operations
type FQueue<'T> = { Front: 'T list; Back: 'T list }

module FQueue =

    /// Empty queue
    let empty<'T> : FQueue<'T> = { Front = []; Back = [] }

    /// Create queue from list
    let ofList (l: 'T list) : FQueue<'T> = { Front = l; Back = [] }

    /// Check if queue is empty
    let isEmpty (q: FQueue<'T>) : bool = (q.Front = [] && q.Back = [])

    /// Enqueue element (O(1) amortized)
    let enqueue (x: 'T) (q: FQueue<'T>) : FQueue<'T> = { q with Back = x :: q.Back }

    /// Try to dequeue element
    let tryDequeue (q: FQueue<'T>) : ('T * FQueue<'T>) option =
        match q.Front with
        | h :: t -> Some(h, { q with Front = t })
        | [] ->
            match List.rev q.Back with
            | [] -> None
            | h :: t -> Some(h, { Front = t; Back = [] })
