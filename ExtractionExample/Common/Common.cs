namespace ExtractionExample
{
    public class Common
    {
        /// <summary>
        /// Very simple displays the current progress of something, without complicated stuff
        /// TODO: Make this into a generic 'console' function, which can be used for displaying percentages on other tasks as well
        /// </summary>
        public static void ConsoleProgressDisplay(string title, int this_step, int total_steps, string unit_of_measurement)
        {
            string spinner = string.Empty;

            switch (this_step % 4)
            {
                case 0: spinner = ("/"); break;
                case 1: spinner = ("-"); break;
                case 2: spinner = ("\\"); break;
                case 3: spinner = ("|"); break;
            }

            if (this_step < total_steps)
            {
                Console.Write($"{title} {spinner} {total_steps} {unit_of_measurement} ... {this_step}\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b");
            }
            else
            {
                Console.WriteLine($"{title} √ {total_steps} {unit_of_measurement}"); // finished (and terminate the line)
            }
        }
    }
}
