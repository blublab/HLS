using System;

namespace Util.Common.Exceptions
{
    /// <summary>
    /// Kapselt ein technisches Problem.
    /// </summary>
    public class TechnicalProblemException : Exception
    {
        public TechnicalProblemException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public TechnicalProblemException(Exception innerException)
            : base("A technical problem occurred.", innerException)
        {
        }
    }
}
