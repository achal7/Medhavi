/// PO-S-060 / PO-S-061 — Supplier Collaboration Health Assessment Policy
/// Traces to: Supply Intelligence Specification §8 (PO-S-060)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCollaborationHealth.Policies

type SupplierCollaborationHealthPolicy =
    { MaxUnassessedSuppliersRatio: decimal
      HighConfidenceThresholdRatio: decimal }

let defaultPolicy : SupplierCollaborationHealthPolicy =
    { MaxUnassessedSuppliersRatio = 0.10m
      HighConfidenceThresholdRatio = 0.80m }
