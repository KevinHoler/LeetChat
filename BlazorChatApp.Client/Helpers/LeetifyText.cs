namespace BlazorChatApp.Client.Helpers
{
    public static class LeetifyText
    {
        public static string Leetify(string text)
        {
            var leetText = text.ToCharArray();
            for (int i = 0; i < leetText.Length; i++)
            {
                leetText[i] = leetText[i] switch
                {
                    'a' or 'A' => '4',
                    'b' or 'B' => '8',
                    'c' or 'C' => '(',
                    'e' or 'E' => '3',
                    'g' or 'G' => '6',
                    'h' or 'H' => '#',
                    'i' or 'I' => '1',
                    'o' or 'O' => '0',
                    's' or 'S' => '5',
                    't' or 'T' => '7',
                    _ => leetText[i]
                };
            }
            return new string(leetText);
        }
    }
}
