from typing import Any, Text, Dict, List
from rasa_sdk.events import SlotSet
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher

class ActionReceiveTopping(Action):
    def name(self) -> Text:
        return "action_receive_topping"

    def run(self, dispatcher: CollectingDispatcher, 
            tracker: Tracker, 
            domain: Dict[Text, Any]):
        # get topping value
        text = tracker.latest_message["text"]
        dispatcher.utter_message(text=f"I'll add {text} to your pizza.")

        return []