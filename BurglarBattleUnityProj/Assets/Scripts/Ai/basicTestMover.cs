using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basicTestMover : MonoBehaviour
{
    // Start is called before the first frame update
    private UnityEngine.AI.NavMeshAgent agent;
    private LocalSearchComponent pc;
    private bool trigger = true;
    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        pc = GetComponent<LocalSearchComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (trigger)
        {
            trigger = false;
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            pc = GetComponent<LocalSearchComponent>();

            pc.CreateDynSearch(transform.position);
        }
        bool test;
        agent.destination = pc.Search(transform,out test);
        if (test)
        {
            trigger = true;
        }
    }
    
}
