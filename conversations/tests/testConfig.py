import unittest
import os
from imp import load_source

class TestConfig(unittest.TestCase):

	def setUp(self):
		self.config = load_source('config', os.path.abspath('..\config.py'))

	def test_db_connection(self):
		print(self.config.SQLALCHEMY_DATABASE_URI)

if __name__ == '__main__':
	unittest.main()