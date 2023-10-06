# -*- coding: utf-8 -*-
import tiktoken
print('result:',tiktoken.encoding_for_model("gpt-3.5-turbo-0301").encode(input(), allowed_special="all").__len__() + 7)