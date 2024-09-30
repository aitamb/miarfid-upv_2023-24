from typing import Any, Text, Dict, List
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher
from rasa_sdk.events import SlotSet
from csv import reader
import random; random.seed = 43
import requests
from urllib.parse import urlencode

base_url = 'http://www.omdbapi.com/?'
api_key = 'bb2ef16e'

# FUNCIONES BUSQUEDA DE PELICULA

def search_movie(director='', year='', rating='', genre=''):
    params = {
        'apikey': api_key,
        'y': year,
        't': director,
        'type': 'movie',
        'genre': genre,
        'rating': rating
    }
    base_url = 'http://www.omdbapi.com/?'
    url = base_url + urlencode(params)
    print(url)
    response = requests.get(url)
    data = response.json()
    return data

def random_imdb_movie():
    # create random id with 9 digits, 2 letters and 7 numbers 
    id = ''.join(random.choice('abcdefghijklmnopqrstuvwxyz') for i in range(2)) + ''.join(random.choice('0123456789') for i in range(7))
    params = {
        'apikey': api_key,
        'i': id
    }
    base_url = 'http://www.omdbapi.com/?'
    url = base_url + urlencode(params)
    response = requests.get(url)
    data = response.json()

    return data

# RASA ACTIONS IMPLEMENTATION

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
        match_movies = search_movie(genre=genre, year=year, rating=rating)
        img = match_movies['Poster']
        
        if not match_movies: # if list is empty
            img = None
            msg = "Sorry, I couldn't find any movie with those specifications...\n You can ask for another movie, tho :)"

        else:
            msg = f"Here you have your movie: \n- Title:    {match_movies['Title']}\n- Director: {match_movies['Director']}\n- Year:     {match_movies['Year']}\n- Rating:   {match_movies['imdbRating']}\n- Overview: {match_movies['Plot']}\n"

        dispatcher.utter_message(text=msg, image=img)
        
        return [SlotSet("guided", False)]
    
class ActionRandomMovie(Action):
    def name(self) -> Text:
        return "action_random_movie"
    
    def run(self, dispatcher: CollectingDispatcher,
            tracker: Tracker,
            domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        
        data = random_imdb_movie()
        img = data['Poster']
        msg = f"Here you have your movie: \n- Title:    {data['Title']}\n- Director: {data['Director']}\n- Year:     {data['Year']}\n- Rating:   {data['imdbRating']}\n- Overview: {data['Plot']}\n"

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
        match_movies = search_movie(genre=genre)
        img = match_movies['Poster']
        
        if not match_movies:
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
                msg = f"Here you have your movie: \n- Title:    {match_movies['Title']}\n- Director: {match_movies['Director']}\n- Year:     {match_movies['Year']}\n- Rating:   {match_movies['imdbRating']}\n- Overview: {match_movies['Plot']}\n"
        
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
            if director == mv_director:
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
            if year == mv_year:
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
            if rating <= mv_rating:
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