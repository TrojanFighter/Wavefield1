
using System.Linq.Expressions;
using System.Runtime.Serialization;
using BehaviorTree;

using UnityEngine;
using WaveField;

namespace WaveField.AI
{




    public class Enemy : MonoBehaviour
    {
        private Tree<Enemy> _tree;
        [SerializeField] private GameObject[] _players;
        private GameObject targetedPlayer;
        [SerializeField] private float _movespeed, _ramSpeed;
        [SerializeField] private float _visibilityRange;
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
            _tree = new Tree<Enemy>(new Selector<Enemy>(
                    new Sequence<Enemy>(
                        new IsTargetingPlayer(),
                        new Selector<Enemy>(
                            new Sequence<Enemy>(
                                new Not<Enemy>(new HasTarget()),
                                new ReTargetClosestPlayer()
                            ),
                            /*
                            new Sequence<Enemy>( // We use a sequence here since this is effectively a checklist...
                                // Sequences fail as soon as a child fails so they're a good way to check
                                // a bunch of conditions before doing something
                                //new ReTargetClosestPlayer(),
                                //new IsPlayerInRange(),
                                new IsAttacking(),
                                new RamAttack(),
                                new IsPlayerInRange(),
                                
                                //new IsInDanger(), // If the enemy has taken a lot of damage AND...
                                //new IsPlayerInRange() // the player is in range...
                                //new Flee() // then run away
                            ),*/
                            new Sequence<Enemy>( // We use a sequence here since this is effectively a checklist...
                                // Sequences fail as soon as a child fails so they're a good way to check
                                // a bunch of conditions before doing something
                                //new ReTargetClosestPlayer(),
                                new Not<Enemy>(new IsPlayerInRange()),
                                //new Not<Enemy>(new IsAttacking()),
                                new MoveTowardsTargetedPlayerAction(),
                                //new IsInDanger(), // If the enemy has taken a lot of damage AND...
                                //new IsPlayerInRange() // the player is in range...
                                //new Flee() // then run away
                                
                                new Selector<Enemy>(
                                    new Sequence<Enemy>(
                                        new IsPlayerInRange(),
                                        new Idle(), // Pulse
                                        new Idle(), // Pulse
                                        new Idle(), // Pulse
                                        new Idle(), // Pulse
                                        new Idle(), // Pulse
                                        new Selector<Enemy>(
                                            new Sequence<Enemy>(
                                                new IsHurt(),
                                                new GenerateTargetPoint(),
                                                new BeginFlee()
                                            ),
                                            new Sequence<Enemy>(
                                                new Not<Enemy>(new IsHurt()),
                                                new BeginAttack()
                                            )
                                        )
                                    ),
                                    new Sequence<Enemy>(
                                        new Not<Enemy>(new IsPlayerInRange()),
                                        new Idle()
                                        )
                                )
                            )
                        )
                    ),
                    new Sequence<Enemy>(
                        new Not<Enemy>(new IsTargetingPlayer()),
                        new Selector<Enemy>(
                            /*new Sequence<Enemy>(
                                new Not<Enemy>(new HasTarget()),
                                new ReTargetClosestPlayer()
                            ),*/
                            new Sequence<Enemy>( // We use a sequence here since this is effectively a checklist...
                                // Sequences fail as soon as a child fails so they're a good way to check
                                // a bunch of conditions before doing something
                                //new ReTargetClosestPlayer(),
                                new Not<Enemy>(new IsTargetPointInRange()),
                                new BeginFlee()
                                //new IsInDanger(), // If the enemy has taken a lot of damage AND...
                                //new IsPlayerInRange() // the player is in range...
                                //new Flee() // then run away
                            ),

                            new Sequence<Enemy>( // We use a sequence here since this is effectively a checklist...
                                // Sequences fail as soon as a child fails so they're a good way to check
                                // a bunch of conditions before doing something
                                //new ReTargetClosestPlayer(),
                                new IsTargetPointInRange(),
                                new BeginPursue()
                                //new IsInDanger(), // If the enemy has taken a lot of damage AND...
                                //new IsPlayerInRange() // the player is in range...
                                //new Flee() // then run away
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
            // Draw a red circle around the enemy so we can see the detection radius
            DrawVisibilityRange();
        }

        private void MoveTowardsPlayer()
        {
            Vector2 playerDirection = (targetedPlayer.transform.position - transform.position).normalized;
            var body = GetComponent<Rigidbody2D>();
            body.MovePosition(body.position+ playerDirection * _movespeed*Time.deltaTime);
        }

        private void RamTowardsPlayer()
        {
            Vector2 playerDirection = (targetedPlayer.transform.position - transform.position).normalized;
            var body = GetComponent<Rigidbody2D>();
            Debug.Log("Attacking "+targetedPlayer.gameObject.name);
            body.MovePosition(body.position+ playerDirection * _ramSpeed*Time.deltaTime);
        }

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

        private void SetColor(Color c)
        {
            gameObject.GetComponent<Renderer>().material.color = c;
        }

        private void OnCollisionEnter(Collision coll)
        {
            if (coll.gameObject.GetComponent<BPlayer>() == null) return;
            _health = Mathf.Max(_health - _damagePerHit, 0);
            var body = GetComponent<Rigidbody>();
            var collisionNormal = (transform.position - coll.gameObject.transform.position).normalized;
            body.AddForce(collisionNormal * 7, ForceMode.Impulse);
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

        private class HasTarget : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                return enemy.targetedPlayer != null;
            }
        }

        private class IsHurt : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                return enemy._health < MaxHealth;
            }
        }

        private class IsTargetingPlayer : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                
                Debug.Log("is targetingPlayer: "+enemy.isTargetingPlayer);
                return enemy.isTargetingPlayer;
            }
        }

        private class IsPlayerInRange : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                var playerPos = enemy.targetedPlayer.transform.position;
                var enemyPos = enemy.transform.position;
                return Vector3.Distance(playerPos, enemyPos) < enemy._visibilityRange;
            }
        }

        private class IsTargetPointInRange : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                var targetPos = enemy.targetPoint;
                var enemyPos = enemy.transform.position;
                return Vector3.Distance(targetPos, enemyPos) < enemy._visibilityRange;
            }
        }
        
        private class IsAttacking:Node<Enemy>
        {
             public override bool Update(Enemy enemy)
             {
                 return enemy.isAttacking;
             }
        }

        ///////////////////
        /// Actions
        ///////////////////

        private class ReTargetClosestPlayer : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {

                float closestPlayerDistance = 100f;
                int playerNum = -1;
                for (int i = 0; i < enemy._players.Length; i++)
                {
                    var playerPos = enemy._players[i].transform.position;
                    var enemyPos = enemy.transform.position;
                    float distance = Vector3.Distance(playerPos, enemyPos);
                    if (distance <= closestPlayerDistance)
                    {
                        closestPlayerDistance = distance;
                        playerNum = i;
                    }
                }

                enemy.targetedPlayer = enemy._players[playerNum];
                return true;
            }
        }

        private class GenerateTargetPoint : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                //enemy.SetColor(Color.red);
                enemy.targetPoint = new Vector2(Random.Range(-GlobalDefine.clampX, GlobalDefine.clampX),Random.Range(-GlobalDefine.clampY, GlobalDefine.clampY));
                return true;
            }
        }

        private class MoveTowardsTargetedPlayerAction : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                //enemy.SetColor(Color.red);
                enemy.MoveTowardsPlayer();
                return true;
            }
        }

        private class MoveTowardsTargetedPointAction : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                //enemy.SetColor(Color.red);
                enemy.MoveTowardsTargetPoint();
                return true;
            }
        }

        private class RamAttack : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                //enemy.SetColor(Color.red);
                enemy.RamTowardsPlayer();
                return true;
            }
        }

        private class BeginFlee : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                //enemy.SetColor(Color.yellow);
                //enemy.MoveAwayFromPlayer();
                enemy.isAttacking = false;
                enemy.isTargetingPlayer = false;
                return true;
            }
        }

        private class BeginPursue : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                //enemy.SetColor(Color.yellow);
                //enemy.MoveAwayFromPlayer();
                enemy.isAttacking = false;
                enemy.isTargetingPlayer = true;
                return true;
            }
        }

        private class BeginAttack : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                //enemy.SetColor(Color.red);
                Debug.Log("Begin Attack");
                enemy.isAttacking = true;
                return true;
            }
        }

        private class Idle : Node<Enemy>
        {
            public override bool Update(Enemy enemy)
            {
                //enemy.SetColor(Color.blue);
                return true;
            }
        }
    }
}

