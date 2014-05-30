import unittest
import os
import logging
from imp import load_source

class TestLoggerConfig(unittest.TestCase):
	def setUp(self):
		self.loggerConfig = load_source('loggerConfig', os.path.abspath('..\loggerConfig.py'))
		self.loggerConfig.setConfiguration()

	def testLogging(self):
		# Сообщение отладочное
		logging.debug( u'This is a debug message' )
		# Сообщение информационное
		logging.info( u'This is an info message' )
		# Сообщение предупреждение
		logging.warning( u'This is a warning' )
		# Сообщение ошибки
		logging.error( u'This is an error message' )
		# Сообщение критическое
		logging.critical( u'FATAL!!!' )

if __name__ == '__main__':
	unittest.main()