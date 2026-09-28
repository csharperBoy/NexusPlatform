# HR OrgChart Integration

## Entity

PostInfoView

Backend relation:

id

parentId

## Adapter

postTreeAdapter

Mapping:

getId()

=> item.id

getParentId()

=> item.parentId

## Update Flow

User Drag

    |

validateMove()

    |

moveNode()

    |

Generate UpdatePostCommand

    |

batchUpdatePosts()
