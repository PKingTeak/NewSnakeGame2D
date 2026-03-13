using System;
using System.Collections.Generic;

public static class EventHub //DataManager로 플레이어를 관리하니까 플레이어 관련 이벤트만 관리해서 해당 옵저버 패턴을 사용해도 무방함. 
{
    public static event Action<int> IntegerEvent;
    public static event Action<string> StringEvent;
    
    public static void Subscribe(Action<int> listener) => IntegerEvent += listener;
    public static void Unsubscribe(Action<int> listener) => IntegerEvent -= listener;



    //오버라이드
    public static void Publish(int value) => IntegerEvent?.Invoke(value);
    public static void Publish(string value) => StringEvent?.Invoke(value); 
    
}



/*
만약 여러개의 이벤트를 구별해서 사용해야 하는 경우는 직접 참조를 하는게 더 효율적이다. 
*/