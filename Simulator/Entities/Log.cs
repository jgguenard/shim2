namespace Simulator.Entities
{
  public class Log
  {
    public int LogId { get; set; }
    public int Index { get; set; }
    public string Type { get; set; }
    public string Entity { get; set; }
    public string Value { get; set; }
    public string SimulationId { get; set; }
    public virtual Simulation Simulation { get; set; }
  }
}