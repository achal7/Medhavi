namespace Medhavi.Core.EnterprisePictureManagement

/// ============================================================================
/// CA-C-019 Policies
/// Governed configuration for Enterprise Picture composition and publication.
/// Policy values must originate from governed artifacts, not hardcoded.
/// ============================================================================
/// PO-C-019: Enterprise Picture Composition Policy.
/// Governs the composition and publication behavior of Enterprise Pictures.
type EnterprisePicturePolicy =
    {
        /// Policy identifier for traceability.
        PolicyId: string

        /// Policy version for evolution tracking.
        Version: int

        /// Maximum number of PictureVersions to retain per PlanningScope.
        /// When exceeded, the oldest Superseded versions are candidates for archival.
        MaxRetainedVersions: int

        /// Debounce window duration for material change coalescing (in seconds)
        DebounceWindowSeconds: int

        /// Whether publishing a new version automatically supersedes
        /// any existing Published version.
        AutoSupersedeOnPublish: bool

        /// Whether empty compositions (zero references) are allowed.
        /// Default: false. An Enterprise Picture Version with no references
        /// has no planning value.
        AllowEmptyComposition: bool
    }

module EnterprisePicturePolicy =

    /// Default policy instance.
    /// In production, this MUST be loaded from a governed policy store.
    /// This default exists only for development and testing.
    let defaultPolicy: EnterprisePicturePolicy =
        { PolicyId = "PO-C-019"
          Version = 1
          MaxRetainedVersions = 50
          DebounceWindowSeconds = 60
          AutoSupersedeOnPublish = true
          AllowEmptyComposition = false }
