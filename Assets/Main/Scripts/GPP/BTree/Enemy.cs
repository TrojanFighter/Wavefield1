using BehaviorTree;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Tree<Enemy> _tree;
    [SerializeField] private GameObject _player;
    [SerializeField] private float _speed;
    [SerializeField] private float _visibilityRange;
    [SerializeField] private float _damagePerHit;

    private const float MaxHealth = 10;
    [SerializeField] private float _health = MaxHealth;

    private void Start ()
    {
        // We define the tree and use a selector at the root to pick the high level behavior (i.e. fight, flight or idle)
        _tree = new Tree<Enemy>(new Selector<Enemy>(

            // (highest priority)
            // Flee Behavior
            new Sequence<Enemy>( // We use a sequence here since this is effectively a checklist...
                // Sequences fail as soon as a child fails so they're a good way to check
                // a bunch of conditions before doing something
                new IsInDanger(), // If the enemy has taken a lot of damage AND...
                new IsPlayerInRange(), // the player is in range...
                new Flee() // then run away
            ),

            // Fight Behavior
            // If we don't need to run then fight...
            new Sequence<Enemy>( // Another sequence to check pre-conditions
                new IsPlayerInRange(), // If the player is in range...
                new Attack() // Attack
            ),

            // (lowest priority)
            // Idle behavior
            // The idle behavior is on the bottom of list so if everything else fails we'll end up here
            new Idle()
        ));
    }

    private void Update ()
    {
        // Update the tree by passing it the context that it should use to drive its decisions/act on
        _tree.Update(this);
        // Draw a red circle around the enemy so we can see the detection radius
        DrawVisibilityRange();
    }

    private void MoveTowardsPlayer()
    {
        var playerDirection = (_player.transform.position - transform.position).normalized;
        var body = GetComponent<Rigidbody>();
        body.AddForce(playerDirection * _speed, ForceMode.Impulse);
    }

    private void MoveAwayFromPlayer()
    {
        var fleeDirection = (transform.position - _player.transform.position).normalized;
        var body = GetComponent<Rigidbody>();
        body.AddForce(fleeDirection * _speed, ForceMode.Impulse);
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
    private class IsInDanger : Node<Enemy>
    {
        public override bool Update(Enemy enemy)
        {
            return enemy._health < MaxHealth / 4;
        }
    }

    private class IsPlayerInRange : Node<Enemy>
    {
        public override bool Update(Enemy enemy)
        {
            var playerPos = enemy._player.transform.position;
            var enemyPos = enemy.transform.position;
            return Vector3.Distance(playerPos, enemyPos) < enemy._visibilityRange;
        }
    }

    ///////////////////
    /// Actions
    ///////////////////
    private class Flee : Node<Enemy>
    {
        public override bool Update(Enemy enemy)
        {
            enemy.SetColor(Color.yellow);
            enemy.MoveAwayFromPlayer();
            return true;
        }
    }

    private class Attack : Node<Enemy>
    {
        public override bool Update(Enemy enemy)
        {
            enemy.SetColor(Color.red);
            enemy.MoveTowardsPlayer();
            return true;
        }
    }

    private class Idle : Node<Enemy>
    {
        public override bool Update(Enemy enemy)
        {
            enemy.SetColor(Color.blue);
            return true;
        }
    }
}

