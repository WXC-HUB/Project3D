using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;



namespace Assets.Scripts.BaseUtils
{
    public static class PhysicUtils
    {
        public static Vector3 getProgressInVector3List(List<Vector3> points , float progress)
        {
            Vector3 result = points[0];
            if(points.Count == 1)
            {
                return result;
            }
            float all_leng = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                all_leng += (points[i + 1] - points[i]).magnitude;
            }

            float cur_leng = 0.0000001f;
            for (int i = 0; i < points.Count - 1; i++)
            {
                cur_leng += (points[i + 1] - points[i]).magnitude;
                if((points[i + 1] - points[i]).magnitude <= 0.00001f)
                {
                    continue;
                }
                if(cur_leng >= all_leng * progress)
                {
                    float p_end = cur_leng / all_leng;
                    float p_st = (cur_leng - (points[i + 1] - points[i]).magnitude ) / all_leng;
                    result = points[i] + (points[i + 1] - points[i]) * ((progress - p_st) / (p_end - p_st));

                    break;
                }
            }
            return result;
        }
        public static Vector3 getNewPositionAfterCircleHit2D(Rigidbody2D MoveRB, RaycastHit2D hitInfo) 
        {
            if(MoveRB.GetComponent<CircleCollider2D>() == null)
            {
                Debug.LogError("没有找到合适的Circle碰撞体: " + MoveRB.gameObject.name); 
                return Vector3.zero;
            }

            return getNewPositionAfterCircleHit2D(MoveRB.GetComponent<CircleCollider2D>().radius, hitInfo);
        
        }

        public static Vector3 getNewPositionAfterCircleHit2D(   float rad , RaycastHit2D hitInfo )
        {
            Vector3 start_pos;
            start_pos = hitInfo.point + hitInfo.normal * rad;

            return start_pos;
        }




        public static Vector2 getNewMoveSpeedAfterCircleHit2D( Rigidbody2D MoveRB , RaycastHit2D hitInfo )
        {
            return getNewMoveSpeedAfterCircleHit2D(MoveRB.velocity , hitInfo );
        }

        public static Vector2 getNewMoveSpeedAfterCircleHit2D(Vector2 MoveSpeed, RaycastHit2D hitInfo)
        {
            Vector2 rot_input = MoveSpeed;
            rot_input = Vector2.Reflect(rot_input, hitInfo.normal);

            return rot_input * 0.1f;
        }

        public static Vector2 getMoveSpeedAfterHit( float mass_go , Vector2 v_go , float mass_be , Vector2 v_be , RaycastHit2D hitInfo)
        {
            Vector2 diff = v_be - v_go;
            Debug.Log(diff);
            Vector2 flect_dir = Vector2.Reflect(diff, hitInfo.normal).normalized;
            Debug.Log(flect_dir);
            Debug.Log(string.Format("go:{0} be:{1}", mass_go, mass_be));
            Debug.Log(v_go);
            return v_go + (mass_be / (mass_go + mass_be)) * diff;
        }
    }

    public static class GameUtils
    {
        public static void DrawCircle(Vector2 center , float radius, Color color)
        {
            Debug.DrawLine(center, center + radius * Vector2.left, color, 10000f);
            Debug.DrawLine(center, center + radius * Vector2.right, color, 10000f);
            Debug.DrawLine(center, center + radius * Vector2.up, color, 10000f);
            Debug.DrawLine(center, center + radius * Vector2.down, color, 10000f);
        }
        public static void getAllChilds(GameObject gameObject, ref List<GameObject> aa)
        {
            foreach (Transform child in gameObject.transform)
            {
                aa.Add(child.gameObject);
                getAllChilds(child.gameObject, ref aa);
            }
        }

        public static void getAllChilds<T>(GameObject gameObject, ref List<T> aa) where T : MonoBehaviour
        {
            foreach (Transform child in gameObject.transform)
            {
                //aa.Add(child.gameObject);
                T tx = child.GetComponent<T>();   
                if(tx != null) aa.Add(tx);
                getAllChilds(child.gameObject, ref aa);
            }
        }

        public static Transform FindChildInTransform(Transform parent, string child)
        {
            List<GameObject> s = new List<GameObject>();  
            getAllChilds(parent.gameObject, ref s);
            foreach (var item in s)
            {
                if(item.transform.name == child)
                {
                    return item.transform;
                }
            }

            return null;    
        }

    }

    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        public static T Instance { get; private set; }

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = (T)this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }


    public class Singleton<T> where T : class
    {
        private static T _Instance;
        private static readonly object padlock = new object();
        public static T Instance
        {
            get
            {
                if (null == _Instance)
                {
                    lock (padlock)
                    {
                        if (null == _Instance)
                        {
                            _Instance = Activator.CreateInstance(typeof(T), true) as T;
                        }
                    }
                }
                return _Instance;
            }
        }

    }


}