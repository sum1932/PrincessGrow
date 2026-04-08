namespace DessertKingdom.Core.Domain
{
    public class DialogueLine
    {
        public string Speaker { get; }
        public string Text { get; }
        public string Illustration { get; }
        public DialogueVisualEffect Effect { get; set; }

        public DialogueLine(string speaker, string text, string illustration = null)
        {
            Speaker = speaker;
            Text = text;
            Illustration = illustration;
            Effect = DialogueVisualEffect.None;
        }
    }

    public enum DialogueVisualEffect
    {
        None,
        Shake,
        Flash,
        Fade,
        Sound
    }
}
