namespace zmachine.Library
{
    public interface IIO
    {
        public string? ReadLine();
        public void Write(string str);
        public void WriteLine(string str);
        public System.ConsoleKeyInfo ReadKey();
    }
}
