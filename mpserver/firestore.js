var admin = require('firebase-admin')
var credentials = require('../cred.json')
var dataUser = require('../Assets/userdata')
admin.initializeApp({
  credential: admin.credential.cert(credentials)
})
var firestore = admin.firestore()
var userId = '4'
var qId = 'q11'
var updatedData = {}

// user firestore
let userRef = firestore.collection('users').doc(userId)
userRef.get().then(doc => {
  console.log(doc.data())
  if (doc.data() !== undefined) {
    updatedData['match_lose'] = doc.data()['match_lose']
    updatedData['score'] = dataUser.score + doc.data()['score']
    updatedData['title'] = doc.data()['title']
    updatedData['coins'] = doc.data()['coins']
    updatedData['matches'] = dataUser.matches
    updatedData['avg_score'] = doc.data()['avg_score'] + parseInt(updatedData['score'] / dataUser.matches)
    updatedData['xp'] = doc.data()['xp']
    updatedData['avg_time'] = parseInt(dataUser.timeSpent / dataUser.matches) + doc.data()['avg_time']
    updatedData['wrong'] = dataUser.wrongAnswers + doc.data()['wrong']
    updatedData['correct'] = dataUser.correctAnswers + doc.data()['correct']
    updatedData['match_won'] = dataUser.matches - updatedData['match_lose']
  } else {
    updatedData['match_lose'] = 1
    updatedData['score'] = dataUser.score
    updatedData['title'] = 'beginner'
    updatedData['coins'] = 9
    updatedData['matches'] = dataUser.matches
    updatedData['avg_score'] = parseInt(updatedData['score'] / dataUser.matches)
    updatedData['xp'] = 30
    updatedData['avg_time'] = parseInt(dataUser.timeSpent / dataUser.matches)
    updatedData['wrong'] = dataUser.wrongAnswers
    updatedData['correct'] = dataUser.correctAnswers
    updatedData['match_won'] = dataUser.matches - updatedData['match_lose']
  }
  console.log(updatedData)
  firestore.collection('users').doc(doc.id).set(updatedData)
}).catch(err => {
  console.log('Error getting documents', err)
})

// QA firestore
firestore.collection('questions').doc(qId).set({ 'correct': 7, 'incorrect': 13 })
