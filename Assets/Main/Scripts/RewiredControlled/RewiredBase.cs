
using Rewired;
using UnityEngine;

namespace  WaveField.RewiredBase
{
    public class RewiredBase : MonoBehaviour
    {
        public int playerId = 0; // The Rewired player id of this character

        protected Player player; // The Rewired Player
        
        [System.NonSerialized] // Don't serialize this so the value is lost on an editor script recompile.
        protected bool initialized;
        
        public bool _functionAllowed = true;
        public RewiredBase[] rewiredSons= new RewiredBase[0];
        
        protected Transform _transform;
        
        public virtual bool Initialize() {
            _transform = transform;
            // Get the Rewired Player object for this player.
            player = ReInput.players.GetPlayer(playerId);   
            
            if (rewiredSons != null&&rewiredSons.Length>0)
            {
                for (int i = 0; i < rewiredSons.Length; i++)
                {
                    if (rewiredSons[i] != this)
                    {
                        rewiredSons[i].playerId = playerId;
                        rewiredSons[i].Initialize();
                    }
                }
            }
            initialized = true;
            return true;
        }
        protected virtual void Update() {
            if(!ReInput.isReady) return; // Exit if Rewired isn't ready. This would only happen during a script recompile in the editor.
            if(!initialized) Initialize(); // Reinitialize after a recompile in the editor
            GetInput();
            ProcessInput();
        }

        protected virtual void GetInput()
        {
            
        }
        protected virtual void ProcessInput()
        {
            
        }

        protected virtual void Start()
        {
        }
    }
}
