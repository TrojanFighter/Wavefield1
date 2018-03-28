
using UnityEngine;
using Rewired;

namespace WaveField.RewiredBase
{
    public class PlayerRewiredControl : RewiredBase
    {
        private Vector3 moveVector,turnVector;
        private bool fire;

        public bool bturnwithMouse,blockOnGround=true;
        
        public float _TurnRateEase = .15f;
        
        public float RunSpeed = 12;
        public float Acceleration = 30;

        float _currentSpeedH;
        float _currentSpeedV;
        Vector3 _amountToMove;
        int _totalJumps;
        

        protected CharacterController _characterController;
        //private Rigidbody _rigidBody;

        protected override void Start()
        {
            Initialize();
        }

        public override bool Initialize()
        {
            _characterController = GetComponent<CharacterController>();
            //_rigidBody = GetComponent<Rigidbody>();

            /*
            var cinematics = FindObjectsOfType<ProCamera2DCinematics>();
            for (int i = 0; i < cinematics.Length; i++)
            {
                cinematics[i].OnCinematicStarted.AddListener(() =>
                    {
                        _functionAllowed = false; 
                        _currentSpeedH = 0;
                        _currentSpeedV = 0;
                    });

                cinematics[i].OnCinematicFinished.AddListener(() =>
                    {
                        _functionAllowed = true; 
                    });
            }*/
            
            return base.Initialize();
        }

        protected override void Update() {
            base.Update();
            //GetInput();
            //ProcessInput();
            TurnWithMouse();
            CheckGround();
        }
        
        protected override void GetInput() {
            // Get the input from the Rewired Player. All controllers that the Player owns will contribute, so it doesn't matter
            // whether the input is coming from a joystick, the keyboard, mouse, or a custom controller.

            if (_functionAllowed)
            {
                moveVector.x = player.GetAxis("Horizontal Move"); // get input by name or action id
                moveVector.y = player.GetAxis("Vertical Move");

                turnVector.x = player.GetAxis("Horizontal Turn");
                turnVector.y = player.GetAxis("Vertical Turn");
            }

            //Debug.Log(player.GetAxis("Horizontal Move")+" "+player.GetAxis("Vertical Move"));
            //fire = player.GetButtonDown("Fire");
        }

        protected override void ProcessInput() {
            
            // Process movement
            if(moveVector.x != 0.0f || moveVector.y != 0.0f) {
                
                var targetSpeedH = moveVector.x * RunSpeed;
                _currentSpeedH = IncrementTowards(_currentSpeedH, targetSpeedH, Acceleration);

                var targetSpeedV = moveVector.y * RunSpeed;
                _currentSpeedV = IncrementTowards(_currentSpeedV, targetSpeedV, Acceleration);

                _amountToMove.x = _currentSpeedH;
                _amountToMove.z = _currentSpeedV;
                _amountToMove.y = 0;
                
                //_rigidBody.MovePosition(_rigidBody.position+ _amountToMove * Time.deltaTime);

                _characterController.Move(_amountToMove * Time.deltaTime);
                //_characterController.Move(moveVector * moveSpeed * Time.deltaTime);
            }

            /*
            // Process fire
            if(fire) {
                GameObject bullet = (GameObject)Instantiate(bulletPrefab, transform.position + transform.right, transform.rotation);
                bullet.GetComponent<Rigidbody>().AddForce(transform.right * bulletSpeed, ForceMode.VelocityChange);
            }*/
        }

        private void CheckGround()
        {
            if (blockOnGround&&transform.position.y > 0)
            {
                transform.position=new Vector3(transform.position.x,0,transform.position.z);
            }
        }

        private void TurnWithMouse()
        {
            if (bturnwithMouse)
            {
                if (playerId == 0)
                {
                    var mouse = Input.mousePosition;
                    var screenPoint = Camera.main.WorldToScreenPoint(_transform.localPosition);
                    var offset = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
                    var angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 90-angle, 0), _TurnRateEase);
                }
            }
            else
            {
                var angle = Mathf.Atan2(turnVector.y, turnVector.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 90-angle, 0), _TurnRateEase);
            }
        }
/*
        void Update()
        {
            if (!_functionAllowed)
                return;

            var targetSpeedH = Input.GetAxis("Horizontal") * RunSpeed;
            _currentSpeedH = IncrementTowards(_currentSpeedH, targetSpeedH, Acceleration);

            var targetSpeedV = Input.GetAxis("Vertical") * RunSpeed;
            _currentSpeedV = IncrementTowards(_currentSpeedV, targetSpeedV, Acceleration);

            _amountToMove.x = _currentSpeedH;
            _amountToMove.z = _currentSpeedV;

            _characterController.Move(_amountToMove * Time.deltaTime);
        }*/

        // Increase n towards target by speed
        private float IncrementTowards(float n, float target, float a)
        {
            if (n == target)
            {
                return n;   
            }
            else
            {
                float dir = Mathf.Sign(target - n); 
                n += a * Time.deltaTime * dir;
                return (dir == Mathf.Sign(target - n)) ? n : target;
            }
        }

        public bool PushedBack(Vector3 distance)
        {
            _characterController.Move(distance);
            return true;
        }
    }
}