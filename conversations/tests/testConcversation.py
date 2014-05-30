import unittest

class TestConversation(unittest.TestCase):

	def setUp(self):
		self.conversations = {1:("Hi. How are you?", 2), 2:("Hi. I'm good. Thank you. And you?", None)}

	def test_first(self):
		print(self.conversations)
		self.assertTrue(1 in self.conversations)

if __name__ == '__main__':
	unittest.main()