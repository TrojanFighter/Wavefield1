namespace WaveField.Event
{
	public abstract class GameEvent
	{
		public delegate void Handler(GameEvent e);
	}
	
	public class MouseDownEvent : GameEvent
	{
		public readonly int button;

		public MouseDownEvent(int button)
		{
			this.button = button;
		}
	}
}