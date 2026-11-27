namespace GameSimulator
{
    public partial class GameSimulator
    {
        const double TARGET_PHASE_TIMEOUT = 30; /// 10 sec until the mon ends prep stage without valid target
        const double MIN_MOVE_CHARGE_SPEED = 0.1; // So that bar charge is 0.1 + speed/500 bars per sec
        const double MOVE_CHARGE_CONSTANT = 1 / 500;
        const double EXTRA_MON_WEIGHT = 40; // Each mon will have 40kg extra weight when dealing with movement calcs
    }
}
