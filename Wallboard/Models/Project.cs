namespace Batzendev.Wallboard.Models
{
    public class Project
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Successful { get; set; }

        public int Failed { get; set; }

        public int Running { get; set; }

        public int Total
        {
            get { return this.Successful + this.Failed + this.Running; }
        }

        public double SuccessfulPercent
        {
            get { return (this.Successful / (double)this.Total) * 100; }
        }

        public double FailedPercent
        {
            get { return (this.Failed / (double)this.Total) * 100; }
        }

        public double RunningPercent
        {
            get { return (this.Running / (double)this.Total) * 100; }
        }
    }
}