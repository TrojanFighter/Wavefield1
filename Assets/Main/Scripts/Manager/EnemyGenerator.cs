using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WaveField.BehaviorTree;

namespace WaveField.Enemy
{
    public class EnemyGenerator : MonoBehaviourEX<EnemyGenerator>
    {
        public float generationGap = 5f;
        public GameObject[] enemyPrefab;
        public int generationCounter = 0;
        public GameObject[] _players;

        public Text m_Killed;
        private int killedNum = 0;
        public GameObject m_RestartButton;

        public void RestartGame()
        {
            SceneManager.UnloadScene(0);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Time.timeScale = 1f;
        }

        public void AddKill()
        {
            killedNum++;
            m_Killed.text ="Killed: "+ killedNum.ToString();
        }

        public void StopGame()
        {
            //if (_players[0] == null || _players[1] == null)
            {
                m_RestartButton.SetActive(true);
                Time.timeScale = 0;
            }
        }


        private void Start()
        {
            StartCoroutine(GeneratePredator());
        }

        IEnumerator GeneratePredator()
        {
            while (true)
            {
                yield return new WaitForSeconds(generationGap);
                GenerateRandomPredator();
                GenerationCounterModify();
            }
        }

        void GenerationCounterModify()
        {
            if (generationCounter < 10)
            {
                generationGap = 5f;
            }
            else if (generationCounter < 20)
            {
                generationGap = 4.5f;
            }
            else if (generationCounter < 30)
            {
                generationGap = 4f;
            }
            else if (generationCounter < 40)
            {
                generationGap = 3.5f;
            }
            else
            {
                generationGap = 3f;
            }
        }

        void GenerateRandomPredator()
        {
            generationCounter++;
            int side1 = Random.RandomRange(0, 4);
            
            int type = Random.RandomRange(0, 5);

            Vector3 startPosition = Vector3.zero;
            switch (side1)
            {
                case 0:
                    startPosition =new Vector2(Random.Range(-GlobalDefine.clampX,GlobalDefine.clampX),GlobalDefine.clampY);
                    break;
                case 1:
                    startPosition =new Vector2(Random.Range(-GlobalDefine.clampX,GlobalDefine.clampX),-GlobalDefine.clampY);
                    break;
                case 2:
                    startPosition =new Vector2(-GlobalDefine.clampX,Random.Range(-GlobalDefine.clampY,GlobalDefine.clampY));
                    break;
                case 3:
                    startPosition =new Vector2(GlobalDefine.clampX,Random.Range(-GlobalDefine.clampY,GlobalDefine.clampY));
                    break;
                default:
                    break;
            }


            GameObject enemyGO = Instantiate(enemyPrefab[type], startPosition, Quaternion.identity) as GameObject;
            enemyGO.GetComponent<EnemyBehaviorTreeAI>()._players = _players;
        }
    }
}
