public class TransitionData
{
    public readonly Difficulty difficulty;
    public readonly int score;

    public TransitionData(Difficulty difficulty = null, int score = 0)
    {
        this.difficulty = difficulty;
        this.score = score;
    }
}