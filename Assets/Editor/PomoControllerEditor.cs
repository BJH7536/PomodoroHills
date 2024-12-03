using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(PomoController))]
public class PomoControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();

        // 대상 객체 가져오기
        PomoController pomoController = (PomoController)target;

        // 여백 추가
        EditorGUILayout.Space();

        // 디버그 정보 헤더
        EditorGUILayout.LabelField("Command Queue Info", EditorStyles.boldLabel);

        // currentCommand 표시
        ICommand command = GetPrivateField<ICommand>(pomoController, "currentCommand");

        if (command == null)
        {
            EditorGUILayout.LabelField("Current Command : ");
        }
        else if (command is MoveCommand moveCommand)
        {
            string str = $"MoveCommand {moveCommand.destination.ToString()}";
            EditorGUILayout.LabelField("Current Command : ", str);
        }
        else
        {
            EditorGUILayout.LabelField("Current Command : ", command.ToString());
        }
        
        // commandQueue 표시
        Queue<ICommand> commandQueue = GetPrivateField<Queue<ICommand>>(pomoController, "commandQueue");
        if (commandQueue != null)
        {
            EditorGUILayout.LabelField("Command Queue Count:", commandQueue.Count.ToString());

            if (commandQueue.Count > 0)
            {
                EditorGUI.indentLevel++;
                foreach (var _command in commandQueue)
                {
                    if (_command is MoveCommand _moveCommand)
                    {
                        EditorGUILayout.LabelField($"{_moveCommand.GetType().Name} {_moveCommand.destination}");
                    } 
                    else
                    {
                        EditorGUILayout.LabelField($"{_command.GetType().Name}");
                    }
                    
                }
                EditorGUI.indentLevel--;
            }
        }

        // 실시간 업데이트를 위해 Inspector 다시 그리기
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    // Reflection을 사용하여 private 필드 접근
    private T GetPrivateField<T>(object obj, string fieldName)
    {
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo field = obj.GetType().GetField(fieldName, bindingFlags);
        if (field != null)
        {
            return (T)field.GetValue(obj);
        }
        return default(T);
    }
}
