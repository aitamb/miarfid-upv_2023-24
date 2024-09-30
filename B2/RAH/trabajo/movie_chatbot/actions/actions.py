from typing import Any, Text, Dict, List
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher
from rasa_sdk.events import SlotSet
from csv import reader
import random; random.seed = 43

def readCSV(path):
    rows = []
    with open(path, encoding="utf8") as file:
        csv_reader = reader(file)
        header = next(csv_reader)
        for row in csv_reader:
            rows.append(row)

    return rows

db_path = 'data/database/imdb_top_1000.csv'
MOVIESDB = readCSV(db_path)
    
class ActionGiveMovie(Action):
    def name(self) -> Text:
        return "action_give_movie"
    
    def run(self, dispatcher: CollectingDispatcher,
            tracker: Tracker,
            domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        
        genre = tracker.get_slot("genre")
        year = int(tracker.get_slot("year"))
        rating = float(tracker.get_slot("rating"))
        match_movies = []
        for movie in MOVIESDB:
            mv_genre = movie[5]
            mv_year = int(movie[2])
            mv_rating = float(movie[6])
            y = False
            if genre in mv_genre:
                if year == 1960 and (mv_year < 1960):
                    y = True
                elif year == 1990 and (mv_year < 1990 and mv_year > 1960):
                    y = True
                elif year == 2000 and (mv_year < 2010 and mv_year > 1990):
                    y = True
                elif year == 2010 and (mv_year > 2010):
                    y = True
                if y:
                    if rating <= mv_rating:
                        match_movies.append(movie)
        
        if not match_movies: # if list is empty
            img = None
            msg = "Sorry, I couldn't find any movie with those specifications...\n You can ask for another movie, tho :)"

        else:
            mov = random.choice(match_movies)
            img = mov[0]
            msg = f"Here you have your movie: \n- Title:    {mov[1]}\n- Director: {mov[9]}\n- Year:     {mov[2]}\n- Rating:   {mov[6]}\n- Overview: {mov[7]}\n"
        
        dispatcher.utter_message(text=msg, image=img)
        
        return [SlotSet("guided", False)]
    
class ActionRandomMovie(Action):
    def name(self) -> Text:
        return "action_random_movie"
    
    def run(self, dispatcher: CollectingDispatcher,
            tracker: Tracker,
            domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        
        mov = random.choice(MOVIESDB)
        img = mov[0]
        msg = f"Here you have your movieee: \n- Title:    {mov[1]}\n- Director: {mov[9]}\n- Year:     {mov[2]}\n- Rating:   {mov[6]}\n- Overview: {mov[7]}\n"
        
        dispatcher.utter_message(text=msg, image=img)
        
        return []
    
class ActionTest(Action):
    def name(self) -> Text:
        return "action_test"
    
    def run(self, dispatcher: CollectingDispatcher,
            tracker: Tracker,
            domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        
        msg = tracker.latest_message
        dispatcher.utter_message(text=f"Test {msg}")
        
        return []

class ActionGiveGenre(Action):
    def name(self) -> Text:
        return "action_give_genre"
    
    def run(self, dispatcher: CollectingDispatcher,
            tracker: Tracker,
            domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        
        genre = tracker.get_slot("genre")
        match_movies = []
        for movie in MOVIESDB:
            mv_genre = movie[5]
            if genre in mv_genre.lower():
                match_movies.append(movie)
        
        if not match_movies: # if list is empty
            msg = f"Sorry, I couldn't find any {genre} movies...\nYou can ask for another movie, tho :)"
        else:
            msg = f"Here are some {genre} movies:\n"
            if len(match_movies) >= 3:
                j = 3
            elif len(match_movies) == 2:
                j = 2
            else:
                j = 1
            random.shuffle(match_movies)
            for i in range(j):
                title = match_movies[i][1]
                director = match_movies[i][9]
                msg = msg + f"- {title} BY {director}\n"
        
        dispatcher.utter_message(text=msg)
        
        return []
    
class ActionGiveDirector(Action):
    def name(self) -> Text:
        return "action_give_director"
    
    def run(self, dispatcher: CollectingDispatcher,
            tracker: Tracker,
            domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        
        director = tracker.get_slot("director")
        match_movies = []
        for movie in MOVIESDB:
            mv_director = movie[9]
            if director.lower() == mv_director.lower():
                match_movies.append(movie)
        
        if not match_movies: # if list is empty
            msg = f"Sorry, I couldn't find any movie from {director}...\nYou can ask for another movie, tho :)"
        else:
            msg = f"Here are some movies from {director}:\n"
            if len(match_movies) >= 3:
                j = 3
            elif len(match_movies) == 2:
                j = 2
            else:
                j = 1
            random.shuffle(match_movies)
            for i in range(j):
                title = match_movies[i][1]
                director = match_movies[i][9]
                msg = msg + f"- {title} BY {director}\n"
        
        dispatcher.utter_message(text=msg)

        return []

class ActionGiveYear(Action):
    def name(self) -> Text:
        return "action_give_year"
    
    def run(self, dispatcher: CollectingDispatcher,
            tracker: Tracker,
            domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        
        year = tracker.get_slot("year")
        match_movies = []
        for movie in MOVIESDB:
            mv_year = movie[2]
            if int(year) == int(mv_year):
                match_movies.append(movie)
        
        if not match_movies: # if list is empty
            msg = f"Sorry, I couldn't find any movie of year {year}...\nYou can ask for another movie, tho :)"
        else:
            msg = f"Here are some movies from year {year}:\n"
            if len(match_movies) >= 3:
                j = 3
            elif len(match_movies) == 2:
                j = 2
            else:
                j = 1
            random.shuffle(match_movies)
            for i in range(j):
                title = match_movies[i][1]
                director = match_movies[i][9]
                msg = msg + f"- {title} BY {director}\n"
        
        dispatcher.utter_message(text=msg)

        return []

class ActionGiveRating(Action):
    def name(self) -> Text:
        return "action_give_rating"
    
    def run(self, dispatcher: CollectingDispatcher,
            tracker: Tracker,
            domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        
        rating = float(tracker.get_slot("rating"))
        match_movies = []
        for movie in MOVIESDB:
            mv_rating = float(movie[6])
            if float(rating) <= mv_rating:
                match_movies.append(movie)
        
        if not match_movies: # if list is empty
            msg = f"Sorry, I couldn't find any movie rated with {rating} or above...\nYou can ask for another movie, tho :)"
        else:
            msg = f"Here are some movies rated with {rating} or above:\n"
            if len(match_movies) >= 3:
                j = 3
            elif len(match_movies) == 2:
                j = 2
            else:
                j = 1
            random.shuffle(match_movies)
            for i in range(j):
                title = match_movies[i][1]
                director = match_movies[i][9]
                msg = msg + f"- {title} BY {director}\n"
        
        dispatcher.utter_message(text=msg)

        return []