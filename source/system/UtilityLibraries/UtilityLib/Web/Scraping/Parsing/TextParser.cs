namespace UtilityLib.Web.Scraping.Parsing
{
    public sealed class TextParser
    {
        readonly string text;
        int index;

        public TextParser(string text)
        {
            this.text = text;
        }

        public bool SeekAfterToken(string token)
        {
            // Check if the token is found past the current index.
            int indexOfToken = text.IndexOf(token, index);
            if (indexOfToken == -1) return false;

            // Update index and return true.
            index = indexOfToken + token.Length;
            return true;
        }

        public string GetNextPiece(string beforeToken, string afterToken)
        {
            // Retrieve the piece string.
            SeekAfterToken(beforeToken);
            int endIndex = text.IndexOf(afterToken, index);
            string piece = text.Substring(index, endIndex - index);

            // Update index to the end of the afterToken string.
            index = endIndex + afterToken.Length;

            // Return the piece string.
            return piece;
        }
    }
}