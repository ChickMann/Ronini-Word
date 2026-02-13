using System;
using SmallHedge.AudioManager;
using UnityEngine;

/// <summary>
/// Base class cho tất cả các nhân vật (Player, Enemy).
/// Quản lý Animation state và trạng thái cơ bản.
/// </summary>
[RequireComponent(typeof(Animator)), RequireComponent(typeof(Rigidbody2D))]
public class Actor : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected Animator animator;
    
    [Header("Debug Info")]
    public ActorState currentState;

    public ActorState CurrentState => currentState;

    protected bool _hasNextAttack = false;

    protected virtual void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        Idle();
    }
    

    #region TriggerFlag
   
    /// <summary>
    /// Chuẩn bị cho đòn tấn công tiếp theo (Input buffer).
    /// </summary>
    public virtual void NextAttack()
    {
        _hasNextAttack = true;
    }

    /// <summary>
    /// Hủy bỏ lệnh tấn công đang chờ (dùng khi bị choáng hoặc vào trạng thái đặc biệt).
    /// </summary>
    public virtual void CancelNextAttack()
    {
        _hasNextAttack = false;
        animator.ResetTrigger(AnimID.Attacking);
    }
    

    #endregion

    #region Anim
    /// <summary>
    /// Kích hoạt tấn công nếu cờ _hasNextAttack được bật.
    /// </summary>
    public virtual void Attack()
    {
        if (_hasNextAttack)
        {
            animator.SetTrigger(AnimID.Attacking);
            _hasNextAttack = false;
        }
    }
    public virtual void Idle()
    {
        animator.SetTrigger(AnimID.Idle);
        currentState = ActorState.Idle;
    }
    public virtual void TakeDamage()
    {
        CancelNextAttack();
        animator.SetTrigger(AnimID.TakeDamage);
    }

    public virtual void Running()
    {
        animator.SetTrigger(AnimID.Running);
        currentState = ActorState.Running;
    }

    public virtual void Stopping()
    {
        animator.SetTrigger(AnimID.Stopping);
    }

    public virtual void FightStand()
    {
        animator.SetTrigger(AnimID.FightStand);
        currentState = ActorState.FightStand;
    }

    public virtual void OnFocus()
    {
        animator.SetTrigger(AnimID.Focus);
    }

    public virtual void OnFocusing()
    {
        animator.SetTrigger(AnimID.Focusing);
        currentState = ActorState.Focusing;
    }

    public virtual void BrokenStand(bool isBroken)
    {
            animator.SetBool(AnimID.BrokenStand,isBroken);
            currentState = ActorState.BrokenStand;
            
    }
    public virtual void Die()
    {
        BrokenStand(false);
        animator.SetTrigger(AnimID.Die);
        currentState = ActorState.Dead;
    }

    public void ResetTrigger()
    {
        animator.ResetTrigger(AnimID.FightStand);   
        animator.ResetTrigger(AnimID.Stopping);
        animator.ResetTrigger(AnimID.Idle);
    }

    /// <summary>
    /// Animation Event: Gọi khi kết thúc animation tấn công.
    /// </summary>
    public virtual void OnAnimationFinishAttack()
    {
        
        animator.SetTrigger(AnimID.StopAttack);
        if(currentState == ActorState.Focusing) return;
        currentState = ActorState.FightStand;
    }
  
    #endregion
    

   
    
   
}