using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpriteGroup
{
    public Sprite[] m_Sprites;
    public float m_Framerate;
    public bool m_Loop;
}

public class SpriteAnim : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer m_SpriteRenderer = null;

    [SerializeField]
    SpriteGroup[] m_Animations = null;

    float m_FrameTimer = 0.0f;
    int m_ActiveAnim = 0;
    int m_ActiveFrame = 0;

    public void Play(int animIndex)
    {
        if(animIndex != m_ActiveAnim)
        {
            m_ActiveAnim = animIndex;
            m_ActiveFrame = 0;
            m_FrameTimer = 0.0f;

            m_SpriteRenderer.sprite = m_Animations[m_ActiveAnim].m_Sprites[m_ActiveFrame];
        }
    }

    private void Update()
    {
        m_FrameTimer += Time.deltaTime;
        if(m_FrameTimer >= m_Animations[m_ActiveAnim].m_Framerate)
        {
            if(m_ActiveFrame < m_Animations[m_ActiveAnim].m_Sprites.Length - 1)
            {
                m_ActiveFrame++;
            }
            else if(m_Animations[m_ActiveAnim].m_Loop)
            {
                m_ActiveFrame = 0;
            }

            m_SpriteRenderer.sprite = m_Animations[m_ActiveAnim].m_Sprites[m_ActiveFrame];
            m_FrameTimer = 0.0f;
        }
    }

    bool IsDone()
    {
        return m_Animations[m_ActiveAnim].m_Sprites.Length - 1 == m_ActiveFrame && !m_Animations[m_ActiveAnim].m_Loop;
    }
}
