namespace Redirection.Api.Models
{
    public interface ITextBuilder
    {
        string Text { get; }

        /// <summary>
        /// Sets the subject.
        /// </summary>
        void SetSubject(string subject);

        /// <summary>
        /// Sets the action.
        /// </summary>
        void SetAction(string action);

        /// <summary>
        /// Sets the complement.
        /// </summary>
        void AppendComplement(string complement);
    }
}
