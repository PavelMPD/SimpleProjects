from app import db

class Conversation(db.Model):
    id = db.Column(db.Integer, primary_key = True)
    name = db.Column(db.String(255))
    sentences = db.relationship('Sentence', backref = 'conversation', lazy = 'dynamic')

    def __repr__(self):
        return '<Conversation %r>' % (self.name)

class Sentence(db.Model):
    id = db.Column(db.Integer, primary_key = True)
    text = db.Column(db.String(255))
    order = db.Column(db.Integer)
    conversation_id = db.Column(db.Integer, db.ForeignKey('conversation.id'))
    
    def __repr__(self):
        return '<Sentence %r>' % (self.text)