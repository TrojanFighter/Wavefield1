using UnityEngine;


namespace WaveField.BehaviorTree
{

    public class EnemyBehaviorTreeAI : MonoBehaviour
    {
        private Tree<EnemyBehaviorTreeAI> _tree;
        [SerializeField] private GameObject[] _players;
        private GameObject targetedPlayer;
        [SerializeField] private float _movespeed, _ramSpeed;
        [SerializeField] private float _visibilityRange,_reachRange,_fuzeRange;
        [SerializeField] private float _damagePerHit;

        private bool isTargetingPlayer = true,isAttacking=false;
        private Vector3 targetPoint;

        private const float MaxHealth = 10;
        [SerializeField] private float _health = MaxHealth;


        public void Hit(int hitPoint)
        {
            _health -= hitPoint;
            if (_health <= 0)
            {
                SelfDestory();
            }
        }

        void SelfDestory()
        {
            _tree = null;
            Destroy(gameObject);
        }

        private void Start()
        {
            // We define the tree and use a selector at the root to pick the high level behavior (i.e. fight, flight or idle)
            _tree = new Tree<EnemyBehaviorTreeAI>(
                new Selector<EnemyBehaviorTreeAI>(
                    new Sequence<EnemyBehaviorTreeAI>(
                        //Attacking behavior
                        new IsAttacking(),
                        new Selector<EnemyBehaviorTreeAI>(
                            new Sequence<EnemyBehaviorTreeAI>(
                                // We use a sequence here since this is effectively a checklist...
                                // Sequences fail as soon as a child fails so they're a good way to check
                                // a bunch of conditions before doing something
                                new Not<EnemyBehaviorTreeAI>(new IsTargetPointReached()),
                                
                                new Selector<EnemyBehaviorTreeAI>(
                                    new Sequence<EnemyBehaviorTreeAI>(
                                        new IsFuzeRangeReached(),
                                        new SelfExplosion()
                                        ),
                                    new Sequence<EnemyBehaviorTreeAI>(
                                        new Not<EnemyBehaviorTreeAI>( new IsFuzeRangeReached()),
                                        new RamAttack()
                                    )
                                )

                            ),

                            new Sequence<EnemyBehaviorTreeAI>(
                                new IsTargetPointReached(),
                                new GenerateTargetPoint(),
                                new BeginFlee()
                            )
                        )
                    ),
                    new Sequence<EnemyBehaviorTreeAI>(
                        //Not attacking
                        new Not<EnemyBehaviorTreeAI>(new IsAttacking()),
                        new Selector<EnemyBehaviorTreeAI>(
                            new Sequence<EnemyBehaviorTreeAI>(
                                //Chasing Behavior
                                new IsTargetingPlayer(),
                                new Selector<EnemyBehaviorTreeAI>(
                                    new Sequence<EnemyBehaviorTreeAI>(
                                        //if not targeted one, get one target
                                        new Not<EnemyBehaviorTreeAI>(new HasTarget()),
                                        new ReTargetClosestPlayer()
                                    ),
                                    new Sequence<EnemyBehaviorTreeAI>(
                                        new Not<EnemyBehaviorTreeAI>(new IsPlayerInVisionRange()),
                                        new MoveTowardsTargetedPlayerAction()
                                    ),
                                    new Sequence<EnemyBehaviorTreeAI>(
                                        new IsPlayerInVisionRange(),
                                        new Idle(), // Pulse
                                        new Idle(), // Pulse
                                        new Idle(), // Pulse
                                        new Idle(), // Pulse
                                        new Idle(), // Pulse
                                        new Selector<EnemyBehaviorTreeAI>(
                                            new Sequence<EnemyBehaviorTreeAI>(
                                                new IsHurt(),
                                                new GenerateTargetPoint(),
                                                new BeginFlee()
                                            ),
                                            new Sequence<EnemyBehaviorTreeAI>(
                                                new Not<EnemyBehaviorTreeAI>(new IsHurt()),
                                                new LockPlayerAttackPosition(),
                                                new BeginAttack()
                                            )
                                        )
                                    )
                                )
                            ),
                            new Sequence<EnemyBehaviorTreeAI>(
                                //Flee to random point behavior
                                new Not<EnemyBehaviorTreeAI>(new IsTargetingPlayer()),
                                new Selector<EnemyBehaviorTreeAI>(
                                    new Sequence<EnemyBehaviorTreeAI>( 
                                        new Not<EnemyBehaviorTreeAI>(new IsTargetPointReached()),
                                        new MoveTowardsTargetedPointAction()
                                    ),

                                    new Sequence<EnemyBehaviorTreeAI>( 
                                        new IsTargetPointReached(),
                                        new BeginPursue()
                                    )
                                )

                            )
                        )
                    ),
                    // (lowest priority)
                    // Idle behavior
                    // The idle behavior is on the bottom of list so if everything else fails we'll end up here
                    new Idle()
                )
            );
        }

        private void Update()
        {
            // Update the tree by passing it the context that it should use to drive its decisions/act on
            _tree.Update(this);
            // Draw a red circle around the enemyBehaviorTreeAi so we can see the detection radius
            DrawVisibilityRange();
        }

        private void MoveTowardsPlayer()
        {
            Vector2 playerDirection = (targetedPlayer.transform.position - transform.position).normalized;
            var body = GetComponent<Rigidbody2D>();
            body.MovePosition(body.position+ playerDirection * _movespeed*Time.deltaTime);
        }
/*
        private void RamTowardsPlayer()
        {
            Vector2 playerDirection = (targetedPlayer.transform.position - transform.position).normalized;
            var body = GetComponent<Rigidbody2D>();
            Debug.Log("Attacking "+targetedPlayer.gameObject.name);
            body.MovePosition(body.position+ playerDirection * _ramSpeed*Time.deltaTime);
        }*/

        private void MoveAwayFromPlayer()
        {
            Vector2 fleeDirection = (transform.position - targetedPlayer.transform.position).normalized;
            var body = GetComponent<Rigidbody2D>();
            body.MovePosition(body.position+ fleeDirection * _movespeed*Time.deltaTime);
        }

        private void MoveTowardsTargetPoint()
        {
            Vector2 playerDirection = (targetPoint - transform.position).normalized;
            var body = GetComponent<Rigidbody2D>();
            body.MovePosition(body.position+ playerDirection * _movespeed*Time.deltaTime);
            
        }
        
        private void RamTowardsTargetPoint()
        {
            Vector2 playerDirection = (targetPoint - transform.position).normalized;
            var body = GetComponent<Rigidbody2D>();
            body.MovePosition(body.position+ playerDirection * _ramSpeed*Time.deltaTime);
            
        }

        private void SetColor(Color c)
        {
            gameObject.GetComponent<Renderer>().material.color = c;
        }

        private void OnCollisionEnter(Collision coll)
        {
            /*if (coll.gameObject.GetComponent<BPlayer>() == null) return;
            _health = Mathf.Max(_health - _damagePerHit, 0);
            var body = GetComponent<Rigidbody>();
            var collisionNormal = (transform.position - coll.gameObject.transform.position).normalized;
            body.AddForce(collisionNormal * 7, ForceMode.Impulse);*/
        }

        private float _lastRange;

        private void DrawVisibilityRange()
        {
            if (_lastRange != _visibilityRange)
            {
                var lineRenderer = GetComponent<LineRenderer>();
                var offset = new Vector3(_visibilityRange, 0, 0);
                var numSegments = lineRenderer.positionCount - 1;
                var angleIncrement = 360.0f / numSegments;
                for (var i = 0; i < numSegments; i++)
                {
                    var newPos = Quaternion.Euler(0, 0, angleIncrement * i) * offset;
                    lineRenderer.SetPosition(i, newPos);
                }

                lineRenderer.SetPosition(numSegments, offset);
                _lastRange = _visibilityRange;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // NODES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////
        // Conditions
        ////////////////////

        private class HasTarget : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                return enemyBehaviorTreeAi.targetedPlayer != null;
            }
        }

        private class IsHurt : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                return enemyBehaviorTreeAi._health < MaxHealth;
            }
        }

        private class IsTargetingPlayer : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                
                //Debug.Log("is targetingPlayer: "+enemyBehaviorTreeAi.isTargetingPlayer);
                return enemyBehaviorTreeAi.isTargetingPlayer;
            }
        }

        private class IsPlayerInVisionRange : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                var playerPos = enemyBehaviorTreeAi.targetedPlayer.transform.position;
                var enemyPos = enemyBehaviorTreeAi.transform.position;
                //Debug.Log("Player In Range: "+ (Vector3.Distance(playerPos, enemyPos) < enemyBehaviorTreeAi._visibilityRange));
                return Vector3.Distance(playerPos, enemyPos) < enemyBehaviorTreeAi._visibilityRange;
            }
        }

        private class IsTargetPointReached : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                var targetPos = enemyBehaviorTreeAi.targetPoint;
                var enemyPos = enemyBehaviorTreeAi.transform.position;
                return Vector3.Distance(targetPos, enemyPos) < enemyBehaviorTreeAi._reachRange;
            }
        }
        
        private class IsFuzeRangeReached : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                var targetPos = enemyBehaviorTreeAi.targetedPlayer.transform.position;
                var enemyPos = enemyBehaviorTreeAi.transform.position;
                return Vector3.Distance(targetPos, enemyPos) < enemyBehaviorTreeAi._fuzeRange;
            }
        }
        
        private class IsAttacking:Node<EnemyBehaviorTreeAI>
        {
             public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
             {
                 return enemyBehaviorTreeAi.isAttacking;
             }
        }

        ///////////////////
        /// Actions
        ///////////////////
        

        private class ReTargetClosestPlayer : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {

                float closestPlayerDistance = 100f;
                int playerNum = -1;
                for (int i = 0; i < enemyBehaviorTreeAi._players.Length; i++)
                {
                    var playerPos = enemyBehaviorTreeAi._players[i].transform.position;
                    var enemyPos = enemyBehaviorTreeAi.transform.position;
                    float distance = Vector3.Distance(playerPos, enemyPos);
                    if (distance <= closestPlayerDistance)
                    {
                        closestPlayerDistance = distance;
                        playerNum = i;
                    }
                }

                enemyBehaviorTreeAi.targetedPlayer = enemyBehaviorTreeAi._players[playerNum];
                return true;
            }
        }

        private class GenerateTargetPoint : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                //enemyBehaviorTreeAi.SetColor(Color.red);
                enemyBehaviorTreeAi.targetPoint = new Vector2(Random.Range(-GlobalDefine.clampX, GlobalDefine.clampX),Random.Range(-GlobalDefine.clampY, GlobalDefine.clampY));
                return true;
            }
        }

        private class MoveTowardsTargetedPlayerAction : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                //enemyBehaviorTreeAi.SetColor(Color.red);
                enemyBehaviorTreeAi.MoveTowardsPlayer();
                return true;
            }
        }

        private class MoveTowardsTargetedPointAction : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                //enemyBehaviorTreeAi.SetColor(Color.red);
                enemyBehaviorTreeAi.MoveTowardsTargetPoint();
                return true;
            }
        }

        private class RamAttack : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                //enemyBehaviorTreeAi.SetColor(Color.red);
                enemyBehaviorTreeAi.RamTowardsTargetPoint();
                return true;
            }
        }

        private class BeginFlee : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                //enemyBehaviorTreeAi.SetColor(Color.yellow);
                //enemyBehaviorTreeAi.MoveAwayFromPlayer();
                
                Debug.Log("BeginFlee");
                
                enemyBehaviorTreeAi.isAttacking = false;
                enemyBehaviorTreeAi.isTargetingPlayer = false;
                return true;
            }
        }

        private class BeginPursue : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                //enemyBehaviorTreeAi.SetColor(Color.yellow);
                //enemyBehaviorTreeAi.MoveAwayFromPlayer();
                Debug.Log("BeginPursue");
                
                enemyBehaviorTreeAi.isAttacking = false;
                enemyBehaviorTreeAi.isTargetingPlayer = true;
                return true;
            }
        }
        
        private class LockPlayerAttackPosition : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                //enemyBehaviorTreeAi.SetColor(Color.red);
                
                //Debug.Log("Begin Attack");

                enemyBehaviorTreeAi.targetPoint = enemyBehaviorTreeAi.targetedPlayer.transform.position;
                return true;
            }
        }

        private class BeginAttack : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                //enemyBehaviorTreeAi.SetColor(Color.red);
                Debug.Log("Begin Attack");
                
                enemyBehaviorTreeAi.isAttacking = true;
                return true;
            }
        }
        
        private class SelfExplosion: Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                Debug.Log("Self Exploded");

               
                return true;
            }
        }

        private class Idle : Node<EnemyBehaviorTreeAI>
        {
            public override bool Update(EnemyBehaviorTreeAI enemyBehaviorTreeAi)
            {
                //enemyBehaviorTreeAi.SetColor(Color.blue);
                Debug.Log("Idle for 1 frame");
                return true;
            }
        }
    }
}

