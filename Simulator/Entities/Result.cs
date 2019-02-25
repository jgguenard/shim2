namespace Simulator.Entities
{
  public class Result
  {
    public int ResultId { get; set; }
    public string Name { get; set; }
    public int Favor { get; set; }
    public string SimulationId { get; set; }
    public virtual Simulation Simulation { get; set; }
  }
}