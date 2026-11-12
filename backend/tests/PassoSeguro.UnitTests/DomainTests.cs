using PassoSeguro.Domain;
namespace PassoSeguro.UnitTests;

public class DomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);
    private static Occurrence Create(int difficulty = 3, int days = 0, double latitude = -23.557, double longitude = -46.646, int category = 1) =>
        Occurrence.Create(Guid.NewGuid(), category, "Obstáculo fictício na passagem", difficulty, latitude, longitude, Now.AddDays(-days), Guid.NewGuid(), "test");
    [Theory, InlineData(0), InlineData(6)] public void RejectsInvalidDifficulty(int value) => Assert.Throws<RuleException>(() => Create(value));
    [Theory, InlineData(double.NaN, 0), InlineData(91, 0), InlineData(0, 181)] public void RejectsInvalidCoordinates(double lat, double lon) => Assert.Throws<RuleException>(() => Create(latitude: lat, longitude: lon));
    [Fact] public void RejectsMissingCategory() => Assert.Throws<RuleException>(() => Create(category: 0));
    [Fact] public void RejectsShortDescription() => Assert.Throws<RuleException>(() => Occurrence.Create(Guid.NewGuid(), 1, "curta", 1, 0, 0, Now, Guid.NewGuid(), "test"));
    [Fact] public void AuthorCanEditOpenOccurrence() { var value = Create(); value.Edit(value.AuthorId, 2, "Descrição fictícia atualizada", 5, -23.556, -46.646); Assert.Equal(5, value.Difficulty); Assert.Equal(2, value.CategoryId); }
    [Fact] public void AnotherCitizenCannotEdit() { var value = Create(); Assert.Throws<RuleException>(() => value.EnsureEditable(Guid.NewGuid())); }
    [Fact] public void OnlyManagerCanReview() { var value = Create(); Assert.Throws<RuleException>(() => value.ChangeStatus(false, Guid.NewGuid(), OccurrenceStatus.InReview, Now)); }
    [Fact] public void CannotResolveWithoutReview() { var value = Create(); Assert.Throws<RuleException>(() => value.ChangeStatus(true, Guid.NewGuid(), OccurrenceStatus.Resolved, Now)); }
    [Fact] public void StatusTransitionsCreateHistoryAndPreventEdits() { var value = Create(); value.ChangeStatus(true, Guid.NewGuid(), OccurrenceStatus.InReview, Now); Assert.Throws<RuleException>(() => value.EnsureEditable(value.AuthorId)); value.ChangeStatus(true, Guid.NewGuid(), OccurrenceStatus.Resolved, Now); Assert.Equal(3, value.History.Count); Assert.Equal(OccurrenceStatus.Resolved, value.Status); }
    [Theory, InlineData(1, 0, 12, "Baixa"), InlineData(3, 0, 36, "Média"), InlineData(5, 0, 60, "Alta"), InlineData(5, 30, 75, "Alta")]
    public void ComputesSeverityAndCappedAge(int difficulty, int age, int score, string level) { var result = PriorityCalculator.Calculate(Create(difficulty, age), [], Now); Assert.Equal(score, result.Score); Assert.Equal(level, result.Level); }
    [Fact]
    public void RecurrenceCountsOnlyNearbyRecentUnresolvedSameCategory()
    {
        var value = Create(5); var nearby = Create(); var old = Create(days: 31); var far = Create(latitude: -23.6); var other = Create(category: 2); var resolved = Create(); resolved.ChangeStatus(true, Guid.NewGuid(), OccurrenceStatus.InReview, Now); resolved.ChangeStatus(true, Guid.NewGuid(), OccurrenceStatus.Resolved, Now);
        var result = PriorityCalculator.Calculate(value, [value, nearby, old, far, other, resolved], Now); Assert.Equal(5, result.RecurrencePoints); Assert.Equal(65, result.Score);
    }
    [Fact] public void PriorityCapsAt100() { var value = Create(5, 20); var result = PriorityCalculator.Calculate(value, Enumerable.Range(0, 20).Select(_ => Create()), Now); Assert.Equal(100, result.Score); Assert.Equal("Crítica", result.Level); }
    [Fact] public void ResolvedHasNoQueuePriority() { var value = Create(); value.ChangeStatus(true, Guid.NewGuid(), OccurrenceStatus.InReview, Now); value.ChangeStatus(true, Guid.NewGuid(), OccurrenceStatus.Resolved, Now); Assert.Equal("Resolvida", PriorityCalculator.Calculate(value, [], Now).Level); }
    [Fact] public void HaversineUsesMeters() { Assert.Equal(0, PriorityCalculator.Distance(0, 0, 0, 0)); Assert.InRange(PriorityCalculator.Distance(0, 0, .001, 0), 110, 112); }
}
