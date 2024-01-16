
namespace asp_net_sql.GameEngine;

interface IComponent
{
    bool IsLocked { get; set; }
    void Enable();
    void Disable();
    void Reset();
}
