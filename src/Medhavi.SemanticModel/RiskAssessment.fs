namespace Medhavi.SemanticModel

/// SE-C-030 Risk Assessment
type RiskAssessment =
    { Likelihood: VocabularyEntryId
      Impact: VocabularyEntryId
      AssessmentTime: Timestamp
      Rationale: string option }

module RiskAssessment =
    let create
        (likelihood: VocabularyEntryId)
        (impact: VocabularyEntryId)
        (assessmentTime: Timestamp)
        (rationale: string option)
        : RiskAssessment =
        { Likelihood = likelihood
          Impact = impact
          AssessmentTime = assessmentTime
          Rationale = rationale }
