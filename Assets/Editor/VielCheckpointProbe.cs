using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad]
public static class VielCheckpointProbe
{
    static double next;
    static string previous;
    static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    static object Read(object target, string name) => target.GetType().GetField(name, Flags)?.GetValue(target);
    static VielCheckpointProbe() { EditorApplication.update += Inspect; }
    static void Inspect()
    {
        if (EditorApplication.timeSinceStartup < next) return;
        next = EditorApplication.timeSinceStartup + 2;
        var result = new StringBuilder();
        result.AppendLine("playing=" + Application.isPlaying + " scene=" + SceneManager.GetActiveScene().path);
        foreach (var exit in UnityEngine.Object.FindObjectsByType<SceneController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var flag = Read(exit, "checkpointFlag") as SpriteRenderer;
            var boss = Read(exit, "checkpointBossSpawner") as EnemySpawner;
            var volume = exit.GetComponent<Collider2D>();
            result.AppendLine("EXIT " + exit.name + " active=" + exit.isActiveAndEnabled + " pos=" + exit.transform.position + " trivia=" + Read(exit,"requireTrivia") + " boss=" + (boss == null ? "null/dead" : boss.name + ":defeated=" + typeof(EnemySpawner).GetProperty("SpawnedEnemyDefeated", Flags).GetValue(boss)) + " loading=" + Read(exit,"loading") + " buildIndex=" + exit.gameObject.scene.buildIndex);
            result.AppendLine("FLAG " + (flag == null ? "missing" : flag.transform.position + " enabled=" + flag.enabled + " active=" + flag.gameObject.activeInHierarchy + " bounds=" + flag.bounds));
            result.AppendLine("VOLUME " + (volume == null ? "missing" : volume.bounds + " enabled=" + volume.enabled + " trigger=" + volume.isTrigger));
            foreach (var rb in UnityEngine.Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
            {
                if (!rb.CompareTag("Player")) continue;
                result.AppendLine("PLAYER " + rb.name + " pos=" + rb.position + " simulated=" + rb.simulated + " constraints=" + rb.constraints);
                foreach (var c in rb.GetComponentsInChildren<Collider2D>())
                    if (volume != null) result.AppendLine("COLLIDER " + c.name + " tag=" + c.tag + " distance=" + volume.Distance(c).distance + " bounds=" + c.bounds);
            }
        }
        foreach (var d in UnityEngine.Object.FindObjectsByType<WelcomeDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            result.AppendLine("DIALOG " + d.name + " active=" + d.isActiveAndEnabled + " page=" + Read(d,"journalPage") + " continued=" + Read(d,"journalContinued"));
        string text = result.ToString();
        if (text == previous) return;
        previous = text;
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "Viel-Live-Checkpoint.txt"), text);
    }
}