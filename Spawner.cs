using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class Spawner : MonoBehaviour
{
   [SerializeField] GameObject GameOverPanel;
   [SerializeField] TMP_Text FinalScoreText;

   [SerializeField] GameObject tile, bottomTile, StartButton;
   TMP_Text ScoreText;

   List<GameObject> stack;
   bool startedGame, finishedGame;

   public static Spawner instance;

   void Awake()
   {
       if (instance == null) instance = this;
       else Destroy(gameObject);
   }

   void Start()
   {
       ScoreText = GameObject.Find("Score").GetComponent<TMP_Text>();
       stack = new List<GameObject>();
       finishedGame = false;
       startedGame = true;

       stack.Add(bottomTile);
       stack[0].GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);

       CreateTile();
   }

   void Update()
   {
       if (finishedGame || !startedGame)
           return;

       if (Input.touchCount > 0)
       {
           Touch touch = Input.GetTouch(0);
           if (touch.phase == TouchPhase.Began) HandleInput();
       }
       else if (Input.GetMouseButtonDown(0))
       {
           HandleInput();
       }

       if (stack.Count > 1)
       {
           Vector3 upDirection = stack[0].transform.up;
           float tiltAngle = Vector3.Angle(Vector3.up, upDirection);
           if (tiltAngle > 30f)
           {
               GameOver();
           }
       }
   }

   void HandleInput()
   {
       if (stack.Count > 1)
           stack[stack.Count - 1].GetComponent<Tile>().ScaleTile();

       if (finishedGame) return;

       StartCoroutine(MoveCamera());
       ScoreText.text = (stack.Count - 1).ToString();
       CreateTile();
   }

   IEnumerator MoveCamera()
   {
       float moveLength = 1.0f;
       GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
       Transform cameraParent = camera.transform.parent;
       if (cameraParent == null)
       {
           GameObject camHolder = new GameObject("CameraHolder");
           camHolder.transform.position = camera.transform.position;
           camera.transform.SetParent(camHolder.transform);
       }
       camera = GameObject.FindGameObjectWithTag("MainCamera").transform.parent.gameObject;

       while (moveLength > 0)
       {
           float stepLength = 0.1f;
           moveLength -= stepLength;

           camera.transform.Translate(0, stepLength, 0, Space.World);

           float tiltAmount = Mathf.Sin(Time.time * 5f) * (stack.Count * 0.05f);
           camera.transform.rotation = Quaternion.Euler(tiltAmount, 0, 0);

           yield return new WaitForSeconds(0.05f);
       }
   }

   void CreateTile()
   {
       GameObject previousTile = stack[stack.Count - 1];
       GameObject activeTile = Instantiate(tile);
       Tile tileScript = activeTile.GetComponent<Tile>();
       stack.Add(activeTile);

       if (stack.Count > 2)
       {
           float randomX = Random.Range(0.5f, 5f);
           float randomZ = Random.Range(0.5f, 5f);
           activeTile.transform.localScale = new Vector3(randomX, previousTile.transform.localScale.y, randomZ);
       }
       else
       {
           activeTile.transform.localScale = previousTile.transform.localScale;
       }

       Vector3 rayOrigin = previousTile.transform.position + Vector3.up * 10f;
       RaycastHit hit;
       if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 20f))
       {
           activeTile.transform.position = hit.point + Vector3.up * (activeTile.transform.localScale.y / 2);
       }
       else
       {
           activeTile.transform.position = new Vector3(
               previousTile.transform.position.x,
               previousTile.transform.position.y + previousTile.transform.localScale.y,
               previousTile.transform.position.z
           );
       }

       activeTile.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);

tileScript.moveAlongX = stack.Count % 2 == 0;
   }

   public void GameOver()
   {
       if (finishedGame) return;

       finishedGame = true;
       StartButton.SetActive(true);
       Time.timeScale = 0.3f;

       StartCoroutine(ResetAfterSlomo());
       StartCoroutine(EndCamera());

       StartCoroutine(ShowGameOverPanel());
   }

   IEnumerator ShowGameOverPanel()
   {
       yield return new WaitForSecondsRealtime(2.5f);
       Time.timeScale = 1f;

       GameOverPanel.SetActive(true);
       FinalScoreText.text = "SCORE: " + (stack.Count - 2).ToString();
   }

   IEnumerator ResetAfterSlomo()
   {
       yield return new WaitForSecondsRealtime(2f);
       Time.timeScale = 1.0f;
   }

   IEnumerator EndCamera()
   {
       GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
       Vector3 temp = camera.transform.position;
       Vector3 final = new Vector3(temp.x, temp.y - stack.Count * 0.5f, temp.z);
       float cameraSizeFinal = stack.Count * 0.65f;

       while (camera.GetComponent<Camera>().orthographicSize < cameraSizeFinal)
       {
           camera.GetComponent<Camera>().orthographicSize += 0.2f;
           temp = camera.transform.position;
           temp = Vector3.Lerp(temp, final, 0.2f);
           camera.transform.position = temp;
           yield return new WaitForSeconds(0.01f);
       }
       camera.transform.position = final;
   }

   public void startButton()
   {
       if (finishedGame)
       {
           UnityEngine.SceneManagement.SceneManager.LoadScene(0);
       }
       else
       {
           StartButton.SetActive(false);
           startedGame = true;
       }
   }

   public int StackHeight()
   {
       return stack.Count;
   }

   public void RestartButton()
   {
       Time.timeScale = 1f;
       UnityEngine.SceneManagement.SceneManager.LoadScene(0);
   }
}
