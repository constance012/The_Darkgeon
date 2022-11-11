/// <summary>
/// An interface contains virtual enemy's behaviour.
/// </summary>
public interface IEnemyBehaviour
{
    public void Patrol();
    public void ChasePlayer();
    public void Attack();
    public void ResetAttack();
    public void Flip();
}
