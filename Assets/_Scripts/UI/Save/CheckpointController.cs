// using UnityEngine;
// public class CheckpointController : MonoBehaviour
// {
//     public Transform playerTransform;
//     public int index;

//     public Sprite activatedCheckpointSprite;
//     private SpriteRenderer spriteRenderer;

//     private void Awake()
//     {
//         spriteRenderer = GetComponent<SpriteRenderer>();

//         if (playerTransform == null)
//             playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

//         UpdateState();
//     }

//     public void UpdateState()
//     {
//         if (SaveVariables.checkpointIndex >= index)
//         {
//             if (activatedCheckpointSprite != null)
//             {
//                 spriteRenderer.sprite = activatedCheckpointSprite;
//             }
//             if (SaveVariables.checkpointIndex == index)
//             {
//                 playerTransform.position = transform.position;
//             }
//         }
//     }

//     private void OnTriggerEnter2D(Collider2D collision)
//     {
//         if (collision.CompareTag("Player"))
//         {
//             if (index > SaveVariables.checkpointIndex)
//             {
//                 SaveVariables.checkpointIndex = index;
//                 SaveManager.Save();

//                 if (activatedCheckpointSprite != null)
//                 {
//                     spriteRenderer.sprite = activatedCheckpointSprite;
//                 }
//             }
//         }
//     }
// }