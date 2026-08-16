namespace Medhavi.Core.EnterprisePictureManagement

/// PO-C-001: Enterprise Picture Composition & Publication Governance.
/// All thresholds originate here (Rule 5.4); never hardcoded in behaviors/algorithms.
type EnterprisePicturePolicy =
    { PolicyId: string
      Version: int
      /// FS-C-001 scheduled composition cadence
      CompositionIntervalSeconds: int
      /// BA-C-001 materiality thresholds (0.0 .. 1.0), evaluated per reference set
      DemandMaterialityThreshold: decimal
      SupplyMaterialityThreshold: decimal
      InventoryMaterialityThreshold: decimal
      /// Maximum allowed gap between Published versions
      MaxPublicationIntervalSeconds: int
      MaxRetainedVersions: int
      AutoSupersedeOnPublish: bool
      AllowEmptyComposition: bool }

module EnterprisePicturePolicy =
    let defaultPolicy : EnterprisePicturePolicy =
        { PolicyId = "PO-C-001"
          Version = 1
          CompositionIntervalSeconds = 3600
          DemandMaterialityThreshold = 0.05m
          SupplyMaterialityThreshold = 0.05m
          InventoryMaterialityThreshold = 0.05m
          MaxPublicationIntervalSeconds = 86400
          MaxRetainedVersions = 50
          AutoSupersedeOnPublish = true
          AllowEmptyComposition = false }
