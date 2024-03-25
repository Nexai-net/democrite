namespace Redirection.Api.Models
{
    [GenerateSerializer]
    public sealed class TextBuilder : ITextBuilder
    {
        #region Fields

        [Id(0)]
        private string? _subject;

        [Id(1)]
        private string? _action;

        [Id(2)]
        private string? _complement;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBuilder"/> class.
        /// </summary>
        public TextBuilder()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBuilder"/> class.
        /// </summary>
        public TextBuilder(string? subject, string? action, string? complement)
        {
            this._subject = subject;
            this._action = action;
            this._complement = complement;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the text.
        /// </summary>
        public string Text
        {
            get { return this._subject + " " + this._action + " " + this._complement; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void SetSubject(string subject)
        {
            this._subject = subject;
        }

        /// <inheritdoc />
        public void SetAction(string action)
        {
            this._action = action;
        }

        /// <summary>
        /// Sets the complement.
        /// </summary>
        public void AppendComplement(string complement)
        {
            if (!string.IsNullOrEmpty(this._complement))
                this._complement += " ";

            this._complement += complement;
        }

        #endregion
    }
}
