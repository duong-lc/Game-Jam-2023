using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIParallax : MonoBehaviour
{
     // private Vector3 pz;
     // private Vector3 StartPos;
     //
     // private int _screenWidth, _screenHeight;
     // private Vector2 _screenOffsetPercentage;
     //
     // public Vector2 screenOffset;
     // private Vector3 _originalPos;
     //
     // void Start ()
     // {
     //     _screenWidth = Screen.width;
     //     _screenHeight = Screen.height;
     //
     //     _originalPos = transform.position;
     // }
     //
     // void LateUpdate ()
     // {
     //     if (Input.touchCount > 0)
     //     {
     //         Touch touch = Input.GetTouch(0);
     //         _screenOffsetPercentage = new Vector2(touch.position.x/_screenWidth, touch.position.y/_screenHeight);
     //         
     //         transform.position = _originalPos + new Vector3(
     //             screenOffset.x * (1f + _screenOffsetPercentage.x), 
     //             screenOffset.y * (1f + _screenOffsetPercentage.y),
     //             _originalPos.z);
     //     }
     // }
     
     // [Tooltip("Start from thurthest to the nearest object.")]
     // [SerializeField] private GameObject[] ParalaxObjects;
     // [SerializeField] private float MouseSpeedX = 1f, MouseSpeedY = .2f;
     // [SerializeField] private Camera cam;
     //
     // //Paralax effect will be applied as an ofset to the original positions
     // private Vector3[] OriginalPositions;
     //
     // // Start is called before the first frame update
     // void Start()
     // {
     //      Cursor.lockState = CursorLockMode.Confined;
     //
     //      OriginalPositions = new Vector3[ParalaxObjects.Length];
     //      for (int i = 0; i < ParalaxObjects.Length; i++)
     //      {
     //           OriginalPositions[i] = ParalaxObjects[i].transform.position;
     //      }
     // }
     //
     // // Update is called once per frame
     // void FixedUpdate()
     // {
     //      float x, y;
     //      if (Input.touchCount > 0)
     //      {
     //           switch (Input.GetTouch(0).phase)
     //           {
     //                case TouchPhase.Moved:
     //                     x = (Input.GetTouch(0).position.x - (Screen.width / 2)) * MouseSpeedX / Screen.width;
     //                     y = (Input.GetTouch(0).position.y - (Screen.height / 2)) * MouseSpeedY / Screen.height;
     //                     //For each object in ParalaxObjects calculate and applly an offset based on cursor position
     //                     for (int i = 1; i < ParalaxObjects.Length + 1; i++)
     //                     {
     //                          ParalaxObjects[i - 1].transform.position = OriginalPositions[i - 1] +
     //                                                                     (new Vector3(x, y, 0f) * i *
     //                                                                      ((i - 1) - (ParalaxObjects.Length / 2)));
     //                     }
     //                     break;
     //           }
     //          
     //      }
     // }
     
     [SerializeField] private RawImage _img;
     [SerializeField] private float _x, _y;
 
     void Update()
     {
          if(_img != null)
               _img.uvRect = new Rect(_img.uvRect.position + new Vector2(_x,_y) * Time.deltaTime,_img.uvRect.size);
     }

}
