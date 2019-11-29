using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AirSpirit: MonoBehaviour
{
    public enum VeggieState { Hungry, Searching, Eating, Winner}
    public VeggieState m_CurrentState;

    private NavMeshAgent _nav;
    GameObject[] veggiePoints;
    GameObject currentVeggie;
    public GameObject escapePoint;
    private Transform _moveTo;

    int index;

    public bool spiritDead;

    public float targetTime = 2.0f;



    void Start()
    {
        _nav = GetComponent<NavMeshAgent>();

        
        

        m_CurrentState = VeggieState.Hungry;
    }

    void Update()
    {
        switch(m_CurrentState)
        {
            case VeggieState.Hungry:
                targetTime -= Time.deltaTime;

                if (targetTime <= 0.0f)
                {
                    Hungry();
                }
                //do fleeing stuff
                break;
            case VeggieState.Searching:
                
                Searching();
                _nav.SetDestination(_moveTo.position);
                break;
            case VeggieState.Eating:
                Eating();
                _nav.SetDestination(_moveTo.position);
                break;
            case VeggieState.Winner:
                Winner();
                break;
        }

        
    }
    void Hungry ()
    {
        //distance is close to carrot
        m_CurrentState = VeggieState.Searching;
    }

    void Searching()
    {
        Debug.Log(currentVeggie);
        veggiePoints = GameObject.FindGameObjectsWithTag("Vegetable");
        if (currentVeggie == null)
        {
            Debug.Log("New Veggie To Chase");
            if (veggiePoints == null || veggiePoints.Length == 0)
            {
                m_CurrentState = VeggieState.Winner;
            }
            else
            {
                index = Random.Range(0, veggiePoints.Length);
                currentVeggie = veggiePoints[index];
            }
            
        }

        _moveTo = currentVeggie.transform;
        //Debug.Log(_moveTo.position);
    }

    void Eating()
    {
        _moveTo = escapePoint.transform;
        Debug.Log(_moveTo.position);
    }

    void Winner()
    {
        
        //Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Weapon")
        {
            Debug.Log("Kill");
            spiritDead = true;
            Destroy(gameObject);
        }
        
        else if (m_CurrentState == VeggieState.Searching)
        {
            if (other.tag == "Vegetable")
            {

                other.transform.parent = transform;
                other.transform.localPosition = Vector3.zero + new Vector3(0, 1f, 0);
                _moveTo = escapePoint.transform;
                Debug.Log("escape");
                m_CurrentState = VeggieState.Eating;

            }

        }
        else if (m_CurrentState == VeggieState.Eating)
        {
            if (other.tag == "Shrine")
            {
                foreach (Transform child in transform)
                {
                    if (child.tag == "Vegetable")
                        Destroy(child.gameObject);
                }
                currentVeggie = null;
                targetTime = 2.0f;
                m_CurrentState = VeggieState.Hungry;
            }
        }
    }
}