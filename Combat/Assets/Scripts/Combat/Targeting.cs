using UnityEngine;

public class Targeting
{
    public float MaxPerformanceRange = 10f;
    public float MaxSelectionDistance = 16f;

    public bool TargetEnemy(PerformanceEnum performance)
    {
        CombatManager.Instance.PerformanceRange.TryGetValue(performance, out MaxPerformanceRange);// get range of the performance
        //float myDebugDistance = CombatManager.Instance.FindDistance(CombatManager.Instance.Player, CombatManager.Instance.CombatTarget.gameObject);
        //Debug.Log("targetting for: " + performance + ". Our targeting range is " + MaxPerformanceRange + ". Our distance to the target is: " + myDebugDistance);

        //NO SELECTION NO TARGET
        if (CombatManager.Instance.SelectedEnemy == null && CombatManager.Instance.CombatTarget == null)
        {
            Debug.LogWarning("no target. Automatically looking for a target");
            
            if (CombatManager.Instance.ThisEnemyManager.Enemies.Count == 0)
            {
                Debug.LogWarning("there are no enemies in the level");
                return false;
            }

            FindEnemiesInCombatRange();   //refresh the list with enemies in range

            if (CombatManager.Instance.EnemiesInSelectionRange.Count == 0)    //there are enemies, but none is in range
            {
                Debug.LogWarning("none of the enemies in this level is in combat range");
                return false;
            }

            return TargetClosestEnemy();    //there are enemies in selection range, target the closest enemy
        }


        //SELECTION AND TARGET
        else if (CombatManager.Instance.SelectedEnemy != null && CombatManager.Instance.CombatTarget != null)    //we have a selection and a target
        {
            Debug.Log("we have a selection and a target");
            float distancePlayerToSelection = CombatManager.Instance.FindDistance(CombatManager.Instance.Player, CombatManager.Instance.SelectedEnemy.gameObject);
            float distancePlayerToTarget = CombatManager.Instance.FindDistance(CombatManager.Instance.Player, CombatManager.Instance.CombatTarget.gameObject);

            if (distancePlayerToSelection < MaxPerformanceRange && distancePlayerToTarget > MaxSelectionDistance) //if within performance range, we want to target the selected enemy, but only our old target is out of SELECTION range
            {
                CombatManager.Instance.UntargetEnemy();    // get rid of old target

                CombatManager.Instance.CombatTarget = CombatManager.Instance.SelectedEnemy.transform;
                CombatManager.Instance.CombatTarget.gameObject.GetComponent<Chicken>().Targeted();
                return true;
            }
            else  //the selected enemy is out of range. Fall back on the target, but only if it is within range. In the target is out of range too, try to target closest.
            {
                if (distancePlayerToTarget > MaxPerformanceRange) //both the selection and the target are out of range.
                {
                    Debug.LogWarning("our target is out of range, looking for a closer target");
                    CombatManager.Instance.UntargetEnemy();

                    FindEnemiesInCombatRange();

                    if (CombatManager.Instance.EnemiesInSelectionRange.Count == 0)
                    {
                        Debug.LogWarning("none of the enemies in this level is in combat range");
                        return false;
                    }
                    return TargetClosestEnemy();
                }
            }
        }

        // TARGET AND NO SELECTION
        else if (CombatManager.Instance.CombatTarget != null && CombatManager.Instance.SelectedEnemy == null)
        {
            //Is the target still in selection range?
            float distancePlayerToTarget = CombatManager.Instance.FindDistance(CombatManager.Instance.Player, CombatManager.Instance.CombatTarget.gameObject);

            if (distancePlayerToTarget > MaxSelectionDistance) 
            {
                Debug.LogWarning("OUR TARGET IS OUT OF RANGE!!!!! Let's target another");
                CombatManager.Instance.UntargetEnemy();

                FindEnemiesInCombatRange();

                if (CombatManager.Instance.EnemiesInSelectionRange.Count == 0)
                {
                    Debug.LogWarning("none of the enemies in this level is in combat range");
                    return false;
                }
                return TargetClosestEnemy();
            }
            
        }
        //SELECTION AND NO TARGET: 
        else if (CombatManager.Instance.CombatTarget == null && CombatManager.Instance.SelectedEnemy != null)  //we have a selection but no target -> make the selection the target if in range
        {
            Debug.Log("we have a selection but no target.");
            float distancePlayerToSelection = CombatManager.Instance.FindDistance(CombatManager.Instance.Player, CombatManager.Instance.SelectedEnemy.gameObject);


            if (distancePlayerToSelection < MaxSelectionDistance)   // is the target still in range? if so, make it target
            {
                if (distancePlayerToSelection > MaxPerformanceRange)
                {
                    Debug.Log("we are too far away for this performance.");
                    return false;
                }

                CombatManager.Instance.UntargetEnemy();
                CombatManager.Instance.CombatTarget = CombatManager.Instance.SelectedEnemy.transform;
                CombatManager.Instance.CombatTarget.gameObject.GetComponent<Chicken>().Targeted();
                CombatManager.Instance.DeselectEnemy();

            }
            else // we had something selected but it went out of range. Target closest if possible
            {
                Debug.Log("we had something selected but it went out of range. Target closest if possible.");

                CombatManager.Instance.DeselectEnemy();    // get rid of the out of range selection

                FindEnemiesInCombatRange();

                if (CombatManager.Instance.EnemiesInSelectionRange.Count == 0)
                {
                    Debug.LogWarning("none of the enemies in this level is in combat range");
                    return false;
                }

                return TargetClosestEnemy();

                //FindEnemiesInCombatRange();

                //if (CombatManager.Instance.EnemiesInSelectionRange.Count == 0)
                //{
                //    Debug.LogWarning("none of the enemies in this level is in combat range");
                //    return false;
                //}
                //TargetClosestEnemy();
            }
        }
        
        return true;
    }

    public void FindEnemiesInCombatRange()  // find all the enemies within the range for combat (the selection range of 10)
    {
        CombatManager.Instance.EnemiesInSelectionRange.Clear();
        foreach (GameObject enemy in CombatManager.Instance.ThisEnemyManager.Enemies)
        {
            float distance = CombatManager.Instance.FindDistance(CombatManager.Instance.Player.gameObject, enemy);
            if (distance < MaxSelectionDistance)
            {
                CombatManager.Instance.EnemiesInSelectionRange.Add(enemy);
            }
        }
    }

    public bool TargetClosestEnemy()
    {

   //     Debug.Log("target closest enemy");
        float closestDistance = MaxSelectionDistance;
        Transform selectableEnemy = null;
        //TODO: Maybe there is an issue with selecting nothing because of the attack range, while the list says there are enemies in range
        foreach (GameObject go in CombatManager.Instance.EnemiesInSelectionRange)
        {
            float distance = CombatManager.Instance.FindDistance(CombatManager.Instance.Player, go);
            //      Debug.Log("Found an enemy at " + distance + " " + EnemiesInCombatRange.Count);

            if (distance < closestDistance && distance < MaxSelectionDistance * MaxSelectionDistance)
            {
                closestDistance = distance;
                if (closestDistance < MaxPerformanceRange)
                    CombatManager.Instance.CombatTarget = go.transform;
                else
                    selectableEnemy = go.transform;
                //    CombatManager.Instance.SelectedEnemy = go.transform;
                //             Debug.Log("found a closer enemy! " + closestDistance);
            }
            
        }

        if (CombatManager.Instance.CombatTarget != null)
        {
            float myDebugDistance = CombatManager.Instance.FindDistance(CombatManager.Instance.Player, CombatManager.Instance.CombatTarget.gameObject);
            Debug.Log("Our targeting range is " + MaxPerformanceRange + ". Our distance to the target is: " + myDebugDistance);

            CombatManager.Instance.CombatTarget.gameObject.GetComponent<Chicken>().Targeted(); // show the ring
            CombatManager.Instance.DeselectEnemy();    //deselect any enemy we had selected before the targeting
            return true;
        }
        else if (selectableEnemy != null) //we couldnt find a target
        {
            Debug.LogWarning("there are enemies in the area, but our attack does not reach that far");
            selectableEnemy.gameObject.GetComponent<Chicken>().Select();
            return false;
        }
        else
            Debug.LogError("how did we end up here?");
        return false;    //maybe should be false?
    }

}
