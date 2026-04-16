using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAppleQuestStep : QuestStep
{
    private bool appleGrabbed = false;
    public string objectName;
    private bool isStepActive = false;

    private void Awake()
    {
        //ChangeState("False");
    }

    private void OnEnable()
    {
        // Create an event and subscribe to it, when the apple is picked up
        GameEventsManager.instance.itemEvents.onItemGrabbed += AppleGrabbed;
        GameEventsManager.instance.questEvents.onQuestStepChange += QuestStepChange;
        GameEventsManager.instance.itemEvents.onItemTriggerEnter += TriggerEnter;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.itemEvents.onItemGrabbed -= AppleGrabbed;
        GameEventsManager.instance.questEvents.onQuestStepChange -= QuestStepChange;
        GameEventsManager.instance.itemEvents.onItemTriggerEnter -= TriggerEnter;
    }

    private void QuestStepChange(string stepId)
    {
        Debug.Log("GrabAppleQuestStep: " + stepId);
        if (stepId.Contains(this.name))
        {
            isStepActive = true;
        }
    }

    private void AppleGrabbed(ObjectInfo obj)
    {
        if (!appleGrabbed && isStepActive)
        {
            if (obj != null && obj.Data.Name.Equals(objectName))
            {
                appleGrabbed = true;
                Debug.Log(objectName + " grabbed");
            }
        }
    }

    private void TriggerEnter(GameObject obj, Collider collider)
    {
        if (appleGrabbed && collider.tag.Equals("Gus"))
        {
            UpdateState();
            FinishStepQuestStep();
            // When destroyed play a sound and show a particle effect
            Instantiate(Resources.Load("Prefabs/ItemVanishEffect"), obj.transform.position, Quaternion.identity);
            //Destroy(obj);
        }
    }

    private void UpdateState()
    {
        string state = appleGrabbed.ToString();
        ChangeState(state);
    }

    protected override void SetQuestStepState(string state)
    {
        try
        {
            Debug.Log("GrabAppleQuestStep: " + state);
            appleGrabbed = bool.Parse(state);
            UpdateState();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing quest step state (" + state +"): " + e.Message);
        }
    }
}
