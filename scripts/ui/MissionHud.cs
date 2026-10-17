using Godot;
using NeighborhoodWatch.Gameplay.Missions.Domain;

namespace NeighborhoodWatch.Ui;

public partial class MissionHud : CanvasLayer
{
    private Label _tasks = null!;
    private Label _prompt = null!;
    private Label _feedback = null!;

    public override void _Ready()
    {
        _tasks = GetNode<Label>("Tasks");
        _prompt = GetNode<Label>("Prompt");
        _feedback = GetNode<Label>("Feedback");
    }

    public void Render(MissionSession session)
    {
        string[] lines = session.Assigned.Select(mission =>
            $"{(session.IsCompleted(mission.Id) ? "[x]" : "[ ]")} {mission.Title}").ToArray();
        _tasks.Text = $"MISSIONS  {session.CompletedCount}/{session.ProgressTarget}\n" +
            string.Join("\n", lines) + $"\nPoints: {session.CompletionPoints}";
    }

    public void SetPrompt(string text) => _prompt.Text = text;

    public void SetFeedback(string text) => _feedback.Text = text;
}
