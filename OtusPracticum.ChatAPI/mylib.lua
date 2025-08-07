#!lua name=mylib 

local function create_something(keys, args)
  redis.call('set', keys[1], args[1])
  return 'ok'
end

local function get_something(args)
  return redis.call('get', args[1])
end

redis.register_function('create_something', create_something)
redis.register_function('get_something', get_something)

--cat mylib.lua | redis-cli -x FUNCTION LOAD REPLACE