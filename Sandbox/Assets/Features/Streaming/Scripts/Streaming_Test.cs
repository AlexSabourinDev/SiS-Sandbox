using UnityEngine;
using Unity;
using Unity.Entities;
using Unity.Scenes;

public class Streaming_Test : MonoBehaviour
{
    [SerializeField]
    private SubScene m_TestScene = null;

    private void Update()
    {
        if(m_TestScene == null)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.L))
        {
            EntityManager entityManager = World.Active.EntityManager;
            foreach(Entity entity in m_TestScene._SceneEntities)
            {
                entityManager.AddComponentData(entity, new RequestSceneLoaded());
            }
        }
        else if(Input.GetKeyDown(KeyCode.U))
        {
            EntityManager entityManager = World.Active.EntityManager;
            foreach(Entity entity in m_TestScene._SceneEntities)
            {
                entityManager.RemoveComponent<RequestSceneLoaded>(entity);
            }
        }
    }
}
