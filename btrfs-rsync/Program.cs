namespace btrfs_rsync
{
    class Program
    {

        static void Main(string[] args)
        {
            var tool = new Tool();

            tool.Run(args).Wait();            
        }

    }

}
